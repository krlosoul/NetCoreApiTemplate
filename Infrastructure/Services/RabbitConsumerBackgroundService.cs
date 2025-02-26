namespace Infrastructure.Services
{
    using Application.Interfaces.Services;
    using Microsoft.Extensions.Hosting;

    public class RabbitConsumerBackgroundService(IRabbitConsumerService rabbitConsumerService) : BackgroundService
    {
        #region Parameters
        private readonly IRabbitConsumerService _rabbitConsumerService = rabbitConsumerService;
        #endregion

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitConsumerService.OnMessageReceived += async (message) =>
            {
                await Task.Run(() => Console.WriteLine($"RabbitMQ: {message}"));
            };

            _rabbitConsumerService.StartListening();

            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _rabbitConsumerService.Dispose();
            base.Dispose();
        }
    }
}