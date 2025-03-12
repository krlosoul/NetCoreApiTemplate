namespace Api.Configurations
{
    using System.Security.Claims;
    using System.Text.Json;
    using Core.Dtos.SecretsDto;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.IdentityModel.Tokens;

    public static class AuthenticationKeycloakConfiguration
    {
        public static IServiceCollection AddAuthenticationKeycloak(this IServiceCollection services)
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

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            AddRolesToClaims(context.Principal);
                            await Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        private static void AddRolesToClaims(ClaimsPrincipal? principal)
        {
            if (principal?.Identity is not ClaimsIdentity identity) return;

            var realmAccessClaim = principal.FindFirst("realm_access");
            if (realmAccessClaim == null) return;

            using var jsonDoc = JsonDocument.Parse(realmAccessClaim.Value);
            if (jsonDoc.RootElement.TryGetProperty("roles", out var rolesElement) && rolesElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var role in rolesElement.EnumerateArray())
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
                }
            }
        }
    }
}