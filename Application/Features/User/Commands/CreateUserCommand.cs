namespace Application.Features.User.Commands
{
    using Core.Entities;
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Features.User.Dtos;
    using Core.Interfaces.DataAccess;
    using AutoMapper;
    using MediatR;
    using Core.Exceptions;
    using Application.Interfaces.Services;
    using Application.Features.User.Queries;
    using Core.Interfaces.Services;
    using Core.Dtos.ResponsesDto;
    using Core.Messages;
    using Core.Dtos.Blobs;

    public class CreateUserCommand : CreateUserDto, IRequest<Result> { }

    public class CreateUserCommandHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        ICryptoService cryptoService, 
        IRedisCacheService redisCacheService,
        IMinioService minioService,
        ICircuitBreakerService circuitBreakerService,
        IKafkaProducerService kafkaProducerService,
        IHangfireService hangfireService,
        IAlfrescoService alfrescoService) : IRequestHandler<CreateUserCommand, Result>
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        private readonly ICryptoService _cryptoService = cryptoService;
        private readonly IRedisCacheService _redisCacheService = redisCacheService;
        private readonly IMinioService _minioService = minioService;
        private readonly ICircuitBreakerService _circuitBreakerService = circuitBreakerService;
        private readonly IKafkaProducerService _kafkaProducerService = kafkaProducerService;
        private readonly IHangfireService _hangfireService = hangfireService;
        private readonly IAlfrescoService _alfrescoService = alfrescoService;
        #endregion

        public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            const string cacheKey = nameof(GetAllUserQuery);
            GetBlobDto getBlobDto = new();
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await RoleExists(request.RolesId!);
                await UserExists(request);
                var blob = await UploadFile(request);
                getBlobDto.FileName = blob.Name;
                int userId = await CreateUser(request, blob.Name!);
                await CreateUserRole(request, userId);
                await UploadFileAlfresco(request);
                await DeleteCache(cacheKey);
                await _circuitBreakerService.ExecuteAsync<Task>(async () => 
                {
                    await _kafkaProducerService.SendMessageAsync($"Se creo el usuario {request.Email} correctamente.");
                    return Task.CompletedTask;
                });
                _hangfireService.Schedule<IRabbitProducerService>(
                    service => service.SendMessageAsync($"Se creo el usuario {request.Email} correctamente."),
                    TimeSpan.Zero
                );
                await _unitOfWork.CommitTransactionAsync();
                return Result.Success(Message.CreatedSuccessfully("user"));
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                await _minioService.DeleteFileAsync(getBlobDto);
                throw; 
            }
        }

        private async Task RoleExists(IList<int> rolesId)
        {
            foreach(int roleId in rolesId){
                var exists = await _unitOfWork.RoleRepository.AnyAsync(x => x.Id == roleId);
                if (!exists) throw new BadRequestException(Message.NotFoundExists("role", $"{roleId}"));
            }
        }

        private async Task UserExists(CreateUserCommand request)
        {
            var existsEmail = await _unitOfWork.UserRepository.AnyAsync(x => x.Email!.Equals(request.Email) && x.Active);
            if (existsEmail) throw new BadRequestException(Message.AlreadyExists("user", $"{request.Email}"));
        }

        private async Task<int> CreateUser(CreateUserCommand request, string fileName)
        {
            request.Password = _cryptoService.EncryptString(request.Password!);
            var user = _mapper.Map<CreateUserDto, User>(request);
            user.Active = true;
            user.RegistrationDate = DateTime.Now;
            user.PhotoName = fileName;
            bool insert = await _unitOfWork.UserRepository.InsertAsync(user);
            if (!insert) throw new BadRequestException(Message.NotBeRegistered("user"));
            return user.Id;
        }

        private async Task CreateUserRole(CreateUserCommand request, int userId)
        {
            foreach(int roleId in request.RolesId!){
                CreateUserRoleDto createUserRoleDto = new(){
                    UserId = userId,
                    RoleId = roleId
                };
                var userRole = _mapper.Map<CreateUserRoleDto, UserRole>(createUserRoleDto);
                bool insert = await _unitOfWork.UserRoleRepository.InsertAsync(userRole);
                if (!insert) throw new BadRequestException(Message.NotBeLinked("role", "user"));
            }
        }
    
        private async Task<BlobDto> UploadFile(CreateUserCommand request)
        {
            BlobDto blobDto = await _circuitBreakerService.ExecuteAsync(() => _minioService.UploadFileAsync(request));
            return blobDto;
        }

        private async Task<string> UploadFileAlfresco(CreateUserCommand request)
        {
            var fileId = await _alfrescoService.UploadFileAsync(request, "9c06fe46-b381-4edd-86fe-46b3813eddd8");//id de carpeta test de alfresco
            return fileId;
        }

        private async Task DeleteCache(string cacheKey)
        {
            await _redisCacheService.DeleteCacheAsync(cacheKey);
        }
    }
}