namespace Api.Configurations
{
    using Api.Policies;
    using Microsoft.AspNetCore.Authorization;

    public static class PoliciesConfiguration
    {
        public static IServiceCollection AddPolicies(this IServiceCollection services)
        {
            services.AddSingleton<IAuthorizationHandler, AdministratorPolicyHandler>();
            services.AddSingleton<IAuthorizationHandler, EditorPolicyHandler>();
            services.AddSingleton<IAuthorizationHandler, VisorPolicyHandler>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdministratorPolicy", policy =>
                {
                    policy.Requirements.Add(new AdministratorPolicy());
                });

                options.AddPolicy("EditorPolicy", policy =>
                {
                    policy.Requirements.Add(new EditorPolicy());
                });

                options.AddPolicy("VisorPolicy", policy =>
                {
                    policy.Requirements.Add(new VisorPolicy());
                });
            });

            return services;
        }
    }
}