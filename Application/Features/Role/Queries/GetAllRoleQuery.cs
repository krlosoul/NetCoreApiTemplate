namespace Application.Features.Role.Queries
{
    using Core.Exceptions;
    using Application.Features.Role.Dtos;
    using Core.Interfaces.DataAccess;
    using Core.Entities;
    using AutoMapper;
    using MediatR;
    using System.Collections.Generic;
    using Core.Dtos.ResponsesDto;
    using Core.Messages;

    public class GetAllRoleQuery : IRequest<Result<IEnumerable<RoleDto>>> { }

    public class GetRoleQueryHandler(IUnitOfWork unitOfWork, IMapper mapper) : IRequestHandler<GetAllRoleQuery, Result<IEnumerable<RoleDto>>>
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;
        #endregion

        public async Task<Result<IEnumerable<RoleDto>>> Handle(GetAllRoleQuery request, CancellationToken cancellationToken)
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();
            if (roles.Data.Any() != true) throw new NotFoundException(Message.NotFoundInSystem("roles"));
            var map = _mapper.Map<IEnumerable<Role>, IEnumerable<RoleDto>>(roles.Data);

            return Result<IEnumerable<RoleDto>>.Success(map, roles.TotalRecords);
        }
    }
}