namespace Api.Configurations
{
    using Core.Constants;
    using Core.Dtos.AppSettingsDto;

    public static class AppSettingsConfiguration
    {
        public static IServiceCollection AddAppSettings(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<VaultAppSettingDto>(o => configuration.GetSection(AppSettingConstant.Vault).Bind(o));
            services.Configure<SecretAppSettingDto>(o => configuration.GetSection(AppSettingConstant.Secret).Bind(o));
            services.Configure<SwaggerSettingDto>(o => configuration.GetSection(AppSettingConstant.Swagger).Bind(o));

            return services;
        }
    }
}