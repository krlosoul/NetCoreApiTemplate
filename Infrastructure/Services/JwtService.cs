using System;

namespace Infrastructure.Services
{
        using Core.Interfaces.Services;
    using Core.Dtos.Jwt;
    using Core.Dtos.SecretsDto;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using Core.Extensions;

    public class JwtService(JwtSecretDto jwtSecretDto) : IJwtService
    {
        #region Parameters
        private readonly JwtSecretDto _jwtDto = jwtSecretDto;
        #endregion

        public string GenerateToken(ClaimDto claimDto)
        {
            var claimsGenerator = ClaimsGenerator.Generate(claimDto);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtDto?.SecretKey!);
            var expirationTime = (double)_jwtDto?.ExpirationTime!;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claimsGenerator),
                Expires = DateTime.UtcNow.AddSeconds(expirationTime),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _jwtDto?.Issuer,
                Audience = _jwtDto?.Audience
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtDto?.SecretKey!);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _jwtDto?.Issuer,
                    ValidateAudience = true,
                    ValidAudience = _jwtDto?.Audience,
                    ValidateLifetime = true
                }, out _);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ClaimDto GetDataByToken(string? token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validatedToken = (JwtSecurityToken)tokenHandler.ReadToken(token);
            var IdUser = validatedToken.Claims.First(c => c.Type == "UserId").Value;
            var UserName = validatedToken.Claims.First(c => c.Type == "UserName").Value;
            var Roles = validatedToken.Claims.First(c => c.Type == "UserRoles").Value;

            return new ClaimDto { UserId = Convert.ToInt32(IdUser), UserName = UserName, UserRoles = Roles };
        }
    }
}
