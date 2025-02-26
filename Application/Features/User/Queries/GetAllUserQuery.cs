namespace Application.Features.User.Queries
{
    using System.Threading;
    using System.Threading.Tasks;
    using Application.Features.User.Dtos;
    using MediatR;
    using Core.Interfaces.DataAccess;
    using AutoMapper;
    using System.Linq.Expressions;
    using Core.Entities;
    using Core.Exceptions;
    using Application.Interfaces.Services;
    using Core.Dtos.ResponsesDto;
    using Core.Dtos.PaginationsDto;
    using Core.Messages;
    using Core.Dtos.Blobs;
    using Core.Interfaces.Services;

    public class GetAllUserQuery : PaginationDto, IRequest<Result<IEnumerable<GetAllUserDto>>> { }

    public class GetAllUserQueryHandler(
        IUnitOfWork unitOfWork, 
        IMapper mapper, 
        IRedisCacheService redisCacheService,
        IMinioService minioService,
        ICircuitBreakerService circuitBreakerService) : IRequestHandler<GetAllUserQuery, Result<IEnumerable<GetAllUserDto>>>
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;   
        private readonly IRedisCacheService _redisCacheService = redisCacheService;
        private readonly IMinioService _minioService = minioService;
        private readonly ICircuitBreakerService _circuitBreakerService = circuitBreakerService;
        #endregion

        public async Task<Result<IEnumerable<GetAllUserDto>>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
        {   
            const string cacheKey = nameof(GetAllUserQuery);
            var cachedResponse = await GetFromCacheAsync(cacheKey);
            if (cachedResponse == null) {
                cachedResponse = await GetFromDatabaseAsync(request);
                await SetFromCacheAsync(cacheKey, cachedResponse);
            }
            return cachedResponse;
        }
    
        private async Task<Result<IEnumerable<GetAllUserDto>>?> GetFromCacheAsync(string cacheKey)
        {
            return await _circuitBreakerService.ExecuteAsync(() => _redisCacheService.GetCacheAsync<Result<IEnumerable<GetAllUserDto>>>(cacheKey));
        }
    
        private async Task<bool> SetFromCacheAsync(string cacheKey, Result<IEnumerable<GetAllUserDto>> data)
        {
            return await _circuitBreakerService.ExecuteAsync(() => _redisCacheService.SetCacheAsync(cacheKey,data));
        }
   
        private async Task GetBlob(GetAllUserDto getAllUserDto)
        {
            var getBlobDto = new GetBlobDto() {FileName = getAllUserDto.PhotoName};
            var blob = await _circuitBreakerService.ExecuteAsync (() => _minioService.GetFileAsync(getBlobDto));
            getAllUserDto.PhotoUrl = blob.Uri;
        }

        private  async Task<Result<IEnumerable<GetAllUserDto>>> GetFromDatabaseAsync(GetAllUserQuery request)
        {
            var includeConfigurations = new (Expression<Func<User, object>> Include, Expression<Func<object, object>>? ThenInclude)[]
            {
                (user => user.UserRoles!, order => ((UserRole)order).Role!)
            };
            var pagination = () => (request.PageNumber, request.PageSize);
            var users = await  _unitOfWork.UserRepository.GetAllAsync(includeProperties: includeConfigurations, paginationExpr: pagination);
            if (users.Data?.Any() != true) throw new NotFoundException(Message.NotFoundInSystem("users"));
            var map = _mapper.Map<IEnumerable<User>, IEnumerable<GetAllUserDto>>(users.Data);
            foreach(GetAllUserDto ma in map){
                await GetBlob(ma);
            }

            return Result<IEnumerable<GetAllUserDto>>.Success(map,users.TotalRecords);
        }
    }
}