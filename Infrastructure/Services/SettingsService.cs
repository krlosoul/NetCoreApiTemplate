namespace Infrastructure.Services
{
    using Core.Dtos.SecretsDto;
    using Core.Interfaces.Services;
    using Core.Dtos.AppSettingsDto;
    using Microsoft.Extensions.Options;

    public class SettingsService(IOptions<SecretAppSettingDto> secretAppSettingDto, ISecretService secretService) : ISettingsService
    {
        #region Parameters
        private readonly ISecretService _secretService = secretService;
        private readonly SecretAppSettingDto? _secretAppSettingDto = secretAppSettingDto.Value;
        #endregion

        public SerilogSecretDto GetSerilogSecret()
        {
            return _secretService.GetSecretAsync<SerilogSecretDto>(_secretAppSettingDto?.Serilog).GetAwaiter().GetResult();
        }

        public DataBaseSecretDto GetDataBaseSecret()
        {
            return _secretService.GetSecretAsync<DataBaseSecretDto>(_secretAppSettingDto?.DataBase).GetAwaiter().GetResult();
        }
        
        public JwtSecretDto GetJwtSecret()
        {
            return _secretService.GetSecretAsync<JwtSecretDto>(_secretAppSettingDto?.Jwt).GetAwaiter().GetResult();
        }

        public CryptoSecretDto GetCryptoSecret()
        {
            return _secretService.GetSecretAsync<CryptoSecretDto>(_secretAppSettingDto?.Crypto).GetAwaiter().GetResult();
        }

        public CircuitBreakerSecretDto GetCircuitBreakerSecret()
        {
            return _secretService.GetSecretAsync<CircuitBreakerSecretDto>(_secretAppSettingDto?.CircuitBreaker).GetAwaiter().GetResult();
        }

        public MinioSecretDto GetMinioSecret()
        {
            return _secretService.GetSecretAsync<MinioSecretDto>(_secretAppSettingDto?.MinIO).GetAwaiter().GetResult();
        }

        public RedisSecretDto GetRedisSecret()
        {
            return _secretService.GetSecretAsync<RedisSecretDto>(_secretAppSettingDto?.Redis).GetAwaiter().GetResult();
        }

        public KafkaSecretDto GetKafkaSecret()
        {
            return _secretService.GetSecretAsync<KafkaSecretDto>(_secretAppSettingDto?.Kafka).GetAwaiter().GetResult();
        }

        public TwilioSecretDto GetTwilioSecret()
        {
            return _secretService.GetSecretAsync<TwilioSecretDto>(_secretAppSettingDto?.Twilio).GetAwaiter().GetResult();
        }

        public HangfireSecretDto GetHangfireSecret()
        {
            return _secretService.GetSecretAsync<HangfireSecretDto>(_secretAppSettingDto?.Hangfire).GetAwaiter().GetResult();
        }

        public RabbitMQSecretDto GetRabbitMQSecret()
        {
            return _secretService.GetSecretAsync<RabbitMQSecretDto>(_secretAppSettingDto?.RabbitMQ).GetAwaiter().GetResult();
        }
    }
}