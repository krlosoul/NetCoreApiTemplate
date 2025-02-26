namespace Infrastructure.Services
{
    using Core.Dtos.Jwt;
    using Application.Interfaces.Services;
    using Microsoft.AspNetCore.Http;
    using Core.Interfaces.Services;

    public class CurrentUserService(IHttpContextAccessor httpContextAccessor, IJwtService jwtService) : ICurrentUserService
    {
        #region Parameters
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IJwtService _jwtService = jwtService;
        #endregion

        public ClaimDto? GetCurrentUser()
        {
            var token = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString().Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token)) return null;
            var user = _jwtService.GetDataByToken(token);
            return user;
        }
    }
}
