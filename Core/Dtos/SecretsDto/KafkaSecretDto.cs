namespace Core.Dtos.SecretsDto
{
    public class KafkaSecretDto
    {
        public string? BootstrapServers { get; set; }
        public string? Topic { get; set; }
        public string? GroupId { get; set; }
    }
}