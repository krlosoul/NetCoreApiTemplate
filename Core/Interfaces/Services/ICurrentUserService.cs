namespace Core.Interfaces.Services
{
    using Dtos.Jwt;

    public interface ICurrentUserService
    {
        /// <summary>
        /// Get current user.
        /// </summary>
        /// <returns>Claim user.</returns>
        public ClaimDto? GetCurrentUser();
    }
}