namespace Core.Extensions
{
    using Core.Dtos.Jwt;
    using System.Collections.Generic;
    using System.Security.Claims;

    public static class ClaimsGenerator
    {
        public static IEnumerable<Claim> Generate(ClaimDto claimDto)
        {
            var claims = new List<Claim>();

            if (claimDto?.UserId != null)
                claims.Add(new Claim("UserId", claimDto.UserId.ToString()));

            if (claimDto?.UserName != null)
                claims.Add(new Claim("UserName", claimDto.UserName));

            if (claimDto?.UserRoles != null)
                claims.Add(new Claim("UserRoles", claimDto.UserRoles));

            return claims;
        }
    }
}