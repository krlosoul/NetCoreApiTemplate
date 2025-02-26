namespace Api.Configurations
{
    using System.Text;
    using Core.Dtos.SecretsDto;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    public static class AuthenticationJwtConfiguration
    {
        public static IServiceCollection AddAuthenticationJwt(this IServiceCollection services)
        {
            var jwtSecretDto = services.BuildServiceProvider().GetRequiredService<JwtSecretDto>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretDto.SecretKey!))
                };
            });

            return services;
        }
    }
}