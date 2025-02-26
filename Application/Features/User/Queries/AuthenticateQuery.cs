namespace Application.Features.User.Queries
{
    using Core.Interfaces.DataAccess;
    using Application.Interfaces.Services;
    using Core.Exceptions;
    using Application.Features.User.Dtos;
    using MediatR;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Core.Entities;
    using Core.Dtos.Jwt;
    using Core.Dtos.ResponsesDto;
    using Core.Interfaces.Services;
    using Core.Messages;

    public class AuthenticateQuery : AuthRequestDto, IRequest<Result<TokenDto>> { }

    public class AuthenticateQueryHandler(IUnitOfWork unitOfWork, IJwtService jwtService, ICryptoService cryptoService) : IRequestHandler<AuthenticateQuery, Result<TokenDto>>
    {
        #region Properties
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IJwtService _jwtService = jwtService;
        private readonly ICryptoService _cryptoService = cryptoService;
        #endregion

        public async Task<Result<TokenDto>> Handle(AuthenticateQuery request, CancellationToken cancellationToken)
        {
            var token = await AuthenticateAsync(request);
            var tokenDto = new TokenDto() {Token = token };

            return Result<TokenDto>.Success(tokenDto, 1);
        }

        private async Task<string> AuthenticateAsync(AuthenticateQuery request)
        {
            var includeConfigurations = new (Expression<Func<User, object>> Include, Expression<Func<object, object>>? ThenInclude)[]
            {
                (user => user.UserRoles!, null),
            };
            var user = await _unitOfWork.UserRepository.FirstOrDefaultAsync(x => x.Email!.Trim().ToLower().Equals(request.Email!.Trim().ToLower()),includeConfigurations) ?? throw new NotFoundException(Message.ErrorLogin);
            user.Password = _cryptoService.DecryptString(user.Password!.Trim());
            if (!user.Password.Trim().Equals(request.Password!.Trim())) throw new NotFoundException(Message.ErrorLogin);
            var roleIds = user.UserRoles!.Select(userRole => userRole.RoleId);
            string rolesConcatenated = string.Join(",", roleIds);
            ClaimDto claimDto = new()
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                UserRoles = rolesConcatenated
            };
            var token = _jwtService.GenerateToken(claimDto);

            return token;
        }
    }
}