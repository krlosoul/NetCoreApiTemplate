namespace Core.Interfaces.Services
{
    using Core.Dtos.SecretsDto;

    public interface ISettingsService
    {
        /// <summary>
        /// Get serilog data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public SerilogSecretDto GetSerilogSecret();

        /// <summary>
        /// Get database data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public DataBaseSecretDto GetDataBaseSecret();

        /// <summary>
        /// Get jwt data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public JwtSecretDto GetJwtSecret();

        /// <summary>
        /// Get crypto data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public CryptoSecretDto GetCryptoSecret();

        /// <summary>
        /// Get circuit breaker data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public CircuitBreakerSecretDto GetCircuitBreakerSecret();

        /// <summary>
        /// Get minio data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public MinioSecretDto GetMinioSecret();

        /// <summary>
        /// Get redis data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public RedisSecretDto GetRedisSecret();

        /// <summary>
        /// Get kafka data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public KafkaSecretDto GetKafkaSecret();

        /// <summary>
        /// Get alfresco data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public AlfrescoSecretDto GetAlfrescoSecret();

        /// <summary>
        /// Get keycloak data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public KeycloakSecretDto GetKeycloakSecret();

        /// <summary>
        /// Get Twilio data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public TwilioSecretDto GetTwilioSecret();
        
        /// <summary>
        /// Get Hangfire data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public HangfireSecretDto GetHangfireSecret();
        
        /// <summary>
        /// Get RabbitMQ data from vault.
        /// </summary>
        /// <returns>The dto.</returns>
        public RabbitMQSecretDto GetRabbitMQSecret();
    }
}