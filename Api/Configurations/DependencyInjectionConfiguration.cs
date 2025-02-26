namespace Api.Configurations
{
    using Infrastructure;
    using Application;
    using Microsoft.Extensions.DependencyInjection;

    public static class DependencyInjectionConfiguration
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAppSettings(configuration);
            services.AddInfrastructure();
            services.AddBusiness();
            services.AddAuthenticationJwt();
            services.AuthenticationKeycloak();
            services.AddPolicies();

            return services;
        }
    }
}
