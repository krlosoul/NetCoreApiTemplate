namespace Api.Configurations
{
    using Core.Dtos.SecretsDto;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    public static class AuthenticationKeycloakConfiguration
    {
        public static IServiceCollection AuthenticationKeycloak(this IServiceCollection services)
        {
            var keycloakSecret = services.BuildServiceProvider().GetRequiredService<KeycloakSecretDto>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = keycloakSecret.Authority;
                options.Audience = keycloakSecret.Audience;
                options.RequireHttpsMetadata = keycloakSecret.RequireHttpsMetadata;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true
                };
            });

            return services;
        }
    }
}