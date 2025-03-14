namespace Api.Configurations
{
    using Swashbuckle.AspNetCore.SwaggerUI;
    using Api.Filters;
    using Microsoft.OpenApi.Models;
    using Core.Dtos.AppSettingsDto;
    using Microsoft.Extensions.Options;

    public static class SwaggerConfiguration
    {
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var serviceProvider = services.BuildServiceProvider();
                var swaggerSettings = serviceProvider.GetRequiredService<IOptions<SwaggerSettingDto>>().Value;
                options.SwaggerDoc(swaggerSettings.Name, new OpenApiInfo
                {
                    Title = swaggerSettings.Title,
                    Version = swaggerSettings.Version
                });
                options.AddSecurityDefinition("x-api-version", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Name = "x-api-version",
                    Type = SecuritySchemeType.ApiKey,
                    Description = "Especificar la versión de la API en el header 'x-api-version'. Ejemplo: 1.0"
                });
                options.OperationFilter<ApiVersionHeaderFilter>();
                options.AddSecurityDefinition(swaggerSettings.SecurityName, new OpenApiSecurityScheme
                {
                    Description = swaggerSettings.DescriptionToken,
                    Name = swaggerSettings.HeaderName,
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = swaggerSettings.Scheme,
                    BearerFormat = swaggerSettings.BearerFormat
                });
                options.OperationFilter<AuthorizeCheckOperationFilter>();
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));
            });
        }

        public static void AddSwaggerConfiguration(this IApplicationBuilder app, IOptions<SwaggerSettingDto> swaggerSettingDto)
        {
            var settings = swaggerSettingDto.Value;
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = string.Empty;
                c.SwaggerEndpoint(settings.Url, settings.DefinitionName);
                c.DocumentTitle = settings.DocumentTitle;
                c.DocExpansion(DocExpansion.None);
            });
        }
    }
}