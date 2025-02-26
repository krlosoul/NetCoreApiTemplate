namespace Application.Features.Role.Commands
{
    using Core.Entities;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Interfaces.DataAccess;
    using AutoMapper;
    using MediatR;
    using Core.Exceptions;
    using Application.Features.Role.Dtos;
    using Core.Dtos.ResponsesDto;
    using Core.Messages;

    public class CreateRoleCommand : CreateRoleDto, IRequest<Result> { }

    public class CreateRoleCommandHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<CreateRoleCommand, Result>
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        #endregion

        public async Task<Result> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            await RoleExists(request);
            await CreateRole(request);
            
            return Result.Success(Message.CreatedSuccessfully("role"));
        }

        private async Task RoleExists(CreateRoleCommand request)
        {
            var exists = await _unitOfWork.RoleRepository.AnyAsync(x => x.Description!.Equals(request.Description));
            if (exists) throw new BadRequestException(Message.AlreadyExists("role", $"{request.Description}"));
        }

        private async Task CreateRole(CreateRoleCommand request)
        {
            var role = _mapper.Map<CreateRoleDto, Role>(request);
            bool insert = await _unitOfWork.RoleRepository.InsertAsync(role);
            if (!insert) throw new BadRequestException(Message.NotBeRegistered("role"));
        }
    }
}