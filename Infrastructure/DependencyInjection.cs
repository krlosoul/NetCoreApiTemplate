namespace Infrastructure
{
    using Core.Interfaces.Services;
    using Core.Interfaces.DataAccess;
    using Application.Interfaces.Services;
    using Core.Dtos.SecretsDto;
    using Core.Dtos.SerilogDto;
    using Infrastructure.DataAccess;
    using Infrastructure.Services;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Events;
    using Serilog.Sinks.Elasticsearch;
    using Hangfire;
    using Microsoft.AspNetCore.Builder;
    using Hangfire.SqlServer;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Core.Dtos.AppSettingsDto;
    using Microsoft.Extensions.Diagnostics.HealthChecks;

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddVaultSecrets();
            services.AddApplicationHealthChecks();
            services.AddConfigHangdire();
            services.AddDbContext<SampleContext>();
            services.AddSingleton<ICryptoService, CryptoService>();
            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IRedisCacheService, RedisCacheService>();
            services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
            services.AddSingleton<IHangfireService, HangfireService>();
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();
            services.AddSingleton<IKafkaConsumerService, KafkaConsumerService>();
            services.AddSingleton<IRabbitProducerService, RabbitProducerService>();
            services.AddSingleton<IRabbitConsumerService, RabbitConsumerService>();
            services.AddHostedService<KafkaConsumerBackgroundService>();
            services.AddHostedService<RabbitConsumerBackgroundService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<IMinioService, MinioService>();
            services.AddScoped<ITwilioService, TwilioService>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.ConfigureSerilog();

            return services;
        }

        #region Private Method

        private static void ConfigureSerilog(this IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            services.AddSingleton<Serilog.ILogger>(logger);

            services.AddSingleton(provider =>
            {
                var serilogOptions = provider.GetRequiredService<SerilogSecretDto>();

                var loggerConfig = new LoggerConfiguration()
                    .MinimumLevel.Is(MapLogLevel(serilogOptions.MinimumLevel!))
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName();

                AddEnrichments(loggerConfig, serilogOptions);
                AddSinks(loggerConfig, serilogOptions);

                return loggerConfig.CreateLogger();
            });

            services.AddSingleton(provider =>
            {
                var loggerInstance = provider.GetRequiredService<Serilog.ILogger>();
                var factory = LoggerFactory.Create(builder => builder.AddSerilog(loggerInstance, dispose: true));
                return factory;
            });

            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog();
            });
        }

        private static void AddEnrichments(LoggerConfiguration loggerConfig, SerilogSecretDto serilogOptions)
        {
            foreach (var enrichment in serilogOptions?.Enrich!)
            {
                switch (enrichment)
                {
                    case "WithMachineName":
                        loggerConfig.Enrich.WithMachineName();
                        break;
                    case "FromLogContext":
                        loggerConfig.Enrich.FromLogContext();
                        break;
                }
            }

            foreach (var property in serilogOptions?.Properties!)
            {
                loggerConfig.Enrich.WithProperty(property.Key, property.Value);
            }
        }

        private static void AddSinks(LoggerConfiguration loggerConfig, SerilogSecretDto serilogSettings)
        {
            foreach (var writeTo in serilogSettings?.WriteTo!)
            {
                switch (writeTo.Name)
                {
                    case "Console":
                        loggerConfig.WriteTo.Console();
                        break;
                    case "Elasticsearch":
                        var elasticArgs = writeTo.Args;
                        loggerConfig.WriteTo.Elasticsearch(ConfigureElasticSink(elasticArgs!));
                        break;
                }
            }
        }

        private static ElasticsearchSinkOptions ConfigureElasticSink(WriteToArgsDto args)
        {
            var uris = args?.NodeUris?.Select(uri => new Uri(uri)).ToList();

            return new ElasticsearchSinkOptions(uris)
            {
                AutoRegisterTemplate = args?.AutoRegisterTemplate ?? true,
                IndexFormat = args?.IndexFormat,
                NumberOfReplicas = args?.NumberOfReplicas ?? 1,
                NumberOfShards = args?.NumberOfShards ?? 1,
                ConnectionTimeout = TimeSpan.FromSeconds(args?.ConnectionTimeout ?? 5),
                TemplateName = args?.TemplateName ?? "serilog-template",
                BatchPostingLimit = args?.BatchSizeLimit ?? 100,
                FailureCallback = (logEvent, exception) => Console.WriteLine($"Error enviando log: {exception?.Message}"),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog | EmitEventFailureHandling.RaiseCallback,
                OverwriteTemplate = true
            };
        }

        private static LogEventLevel MapLogLevel(string minimumLevel)
        {
            return Enum.TryParse(minimumLevel, true, out LogEventLevel level) ? level : LogEventLevel.Information;
        }

        private static IServiceCollection AddVaultSecrets(this IServiceCollection services)
        {
            services.AddScoped<ISecretService, SecretService>();
            services.AddSingleton<ISettingsService, SettingsService>();

            static T ResolveSecret<T>(IServiceProvider provider, Func<ISettingsService, T> selector) => selector(provider.GetRequiredService<ISettingsService>());

            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetSerilogSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetDataBaseSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetJwtSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetCryptoSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetCircuitBreakerSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetMinioSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetRedisSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetKafkaSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetTwilioSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetHangfireSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetRabbitMQSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetKeycloakSecret()));
            services.AddSingleton(provider => ResolveSecret(provider, s => s.GetAlfrescoSecret()));

            return services;
        }

        public static void AddHangfireDashboard(this IApplicationBuilder app)
        {
            var hangfireSecretDto = app.ApplicationServices.GetRequiredService<HangfireSecretDto>();
            app.UseHangfireDashboard(hangfireSecretDto?.DashboardUrl!);
        }

        private static IServiceCollection AddConfigHangdire(this IServiceCollection services)
        {
            services.AddHangfire((serviceProvider, config) =>
            {
                var hangfireSecretDto = services.BuildServiceProvider().GetRequiredService<HangfireSecretDto>();

                config.SetDataCompatibilityLevel((CompatibilityLevel)Enum.Parse(typeof(CompatibilityLevel), hangfireSecretDto.CompatibilityLevel!))
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(hangfireSecretDto.ConnectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(hangfireSecretDto.CommandBatchMaxTimeoutInMinutes),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(hangfireSecretDto.SlidingInvisibilityTimeoutInMinutes),
                    QueuePollInterval = TimeSpan.FromMilliseconds(hangfireSecretDto.QueuePollIntervalInMilliseconds),
                    UseRecommendedIsolationLevel = hangfireSecretDto.UseRecommendedIsolationLevel,
                    DisableGlobalLocks = hangfireSecretDto.DisableGlobalLocks
                });
            });

            services.AddHangfireServer();

            return services;
        }
        
        private static IServiceCollection AddApplicationHealthChecks(this IServiceCollection services)
        {
            var vaultSecret = services.BuildServiceProvider().GetRequiredService<IOptions<VaultAppSettingDto>>().Value;
            var databaseSecret = services.BuildServiceProvider().GetRequiredService<DataBaseSecretDto>();
            var minioSecret = services.BuildServiceProvider().GetRequiredService<MinioSecretDto>();

            services.AddHealthChecks()
                .AddUrlGroup(new Uri($"{vaultSecret.Uri}/v1/sys/health"), name: "vault")
                .AddSqlServer(databaseSecret.ConnectionString!, name: "sqlserver", failureStatus: HealthStatus.Unhealthy)
                .AddUrlGroup(new Uri($"{minioSecret.Endpoint}/minio/health/live"), name: "minio");

            services.AddHealthChecksUI().AddInMemoryStorage();

            return services;
        }
        #endregion
    }
}