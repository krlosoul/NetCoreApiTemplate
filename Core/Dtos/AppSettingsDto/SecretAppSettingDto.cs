namespace Core.Dtos.AppSettingsDto
{
    public class SecretAppSettingDto
    {
        public string? DataBase { get; set; }
        public string? MinIO { get; set; }
        public string? Jwt { get; set; }
        public string? Redis { get; set; }
        public string? CircuitBreaker { get; set; }
        public string? Serilog { get; set; }
        public string? Crypto { get; set; }
        public string? Kafka { get; set; }
        public string? Twilio { get; set; }
        public string? Hangfire { get; set; }
        public string? RabbitMQ { get; set; }
        public string? Alfresco { get; set; }
        public string? Keycloak { get; set; }
    }
}