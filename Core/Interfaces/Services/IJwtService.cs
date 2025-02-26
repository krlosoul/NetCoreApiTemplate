namespace Core.Interfaces.Services
{
    using Dtos.Jwt;

    public interface IJwtService
    {
        /// <summary>
        /// Generate token by user.
        /// </summary>
        /// <param name="claimDto">The data token.</param>
        /// <returns>Token.</returns>
        string GenerateToken(ClaimDto claimDto);

        /// <summary>
        /// Validate token.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>true if valod, false otherwise.</returns>
        bool ValidateToken(string token);

        /// <summary>
        /// Get user information by token.
        /// </summary>
        /// <param name="token">Token.</param>
        /// <returns>The claim.</returns>
        ClaimDto GetDataByToken(string? token);
    }
}
