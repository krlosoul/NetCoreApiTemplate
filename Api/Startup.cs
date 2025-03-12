namespace Api
{
    using Api.Configurations;
    using Api.Filters;
    using FluentValidation.AspNetCore;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Core.Dtos.AppSettingsDto;
    using Microsoft.Extensions.Options;
    using Api.Middlewares;
    using Infrastructure;
    using Application.Interfaces.Services;
    using Serilog;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using HealthChecks.UI.Client;

    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDependencyInjection(Configuration);

            services.AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>());
            services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
            services.Configure<ApiBehaviorOptions>(options => options.SuppressModelStateInvalidFilter = true);
            services.AddDataProtection();
            services.AddCors(o => o.AddPolicy("AllowAll", builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            }));
            services.AddMvc().AddNewtonsoftJson();
            services.AddSwaggerConfiguration();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SwaggerSettingDto> swaggerSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("AllowAll");
            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.AddHangfireDashboard();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/actuator/health", new HealthCheckOptions
                {
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });

            app.UseHealthChecksUI(config =>
            {
                config.UIPath = "/actuator";
            });
            app.AddSwaggerConfiguration(swaggerSettings);
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var notificationService = serviceProvider.GetRequiredService<ITwilioService>();


                //notificationService.SendWhatsAppAsync("+573116806969", "¡Hola , soy una notificacion enviada desde net core att el mejor desarrollador del mundo!").Wait();

                /*var objeto = new { license_plate = "ABC123", service= "LAVADA DE PRUEBA" };
                notificationService.SendWhatsAppThemeAsync("+573116806969",objeto).Wait();*/
            }
        }
    }
}
