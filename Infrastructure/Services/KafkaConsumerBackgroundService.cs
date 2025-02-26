namespace Infrastructure.Services
{
    using Application.Interfaces.Services;
    using Microsoft.Extensions.Hosting;

    public class KafkaConsumerBackgroundService(IKafkaConsumerService kafkaConsumerService) : BackgroundService
    {
        #region Parameters
        private readonly IKafkaConsumerService _kafkaConsumerService = kafkaConsumerService;
        #endregion

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _kafkaConsumerService.OnMessageReceived += async (message) =>
            {
                await Task.Run(() => Console.WriteLine($"Kafka: {message}"));
            };

            _kafkaConsumerService.StartListening();

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _kafkaConsumerService.Dispose();
            base.Dispose();
        }
    }
}