namespace Infrastructure.Services
{
    using System.Net;
    using System.Threading.Tasks;
    using Application.Interfaces.Services;
    using Confluent.Kafka;
    using Core.Dtos.SecretsDto;

    public class KafkaProducerService(KafkaSecretDto kafkaSecretDto) : IKafkaProducerService
    {
        #region Properties 
        private readonly KafkaSecretDto? _kafkaSecretDto = kafkaSecretDto;
        #endregion

        public async Task SendMessageAsync(string message)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = _kafkaSecretDto?.BootstrapServers,
                ClientId = Dns.GetHostName()
            };
            using var producer = new ProducerBuilder<Null, string>(config).Build();
            var result = await producer.ProduceAsync(_kafkaSecretDto?.Topic, new Message<Null, string> { Value = message });
        }
    }
}