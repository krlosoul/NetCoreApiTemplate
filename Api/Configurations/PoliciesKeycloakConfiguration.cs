namespace Api.Configurations
{
    public static class PoliciesKeycloakConfiguration
    {
        public static IServiceCollection AddPoliciesKeycloak(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Administrador", policy => policy.RequireRole("administrador"));
            });

            return services;
        }
    }
}