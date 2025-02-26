namespace Infrastructure.Services
{
    using System.Text;
    using System.Threading.Tasks;
    using Application.Interfaces.Services;
    using Core.Dtos.SecretsDto;
    using RabbitMQ.Client;

    public class RabbitProducerService : IRabbitProducerService
    {
        #region Properties 
        private readonly RabbitMQSecretDto? _rabbitMQSecretDto;
        private readonly IChannel _channel;
        #endregion

        public RabbitProducerService(RabbitMQSecretDto rabbitMQSecretDto)
        {
            _rabbitMQSecretDto = rabbitMQSecretDto;

            var factory = new ConnectionFactory() { HostName = _rabbitMQSecretDto.HostName!, UserName = _rabbitMQSecretDto.UserName!, Password = _rabbitMQSecretDto.Password! };
            var connection = factory.CreateConnectionAsync().Result;
            _channel = connection.CreateChannelAsync().Result;

            _channel.ExchangeDeclareAsync(
                exchange: _rabbitMQSecretDto.ExchangeName!,
                type: _rabbitMQSecretDto.ExchangeType!,
                durable: _rabbitMQSecretDto.ExchangeDurable,
                autoDelete: _rabbitMQSecretDto.ExchangeAutoDelete,
                arguments: null);
        }

        public async Task SendMessageAsync(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            var props = new BasicProperties();
            await _channel.BasicPublishAsync(
                exchange: _rabbitMQSecretDto?.ExchangeName!,
                routingKey: _rabbitMQSecretDto?.RoutingKey!,
                mandatory: true,
                basicProperties: props,
                body: body);
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
        }
    }
}