namespace Infrastructure.Services
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System;
    using Core.Dtos.SecretsDto;
    using Application.Interfaces.Services;

    public class RabbitConsumerService : IRabbitConsumerService
    {
        #region Properties
        private readonly RabbitMQSecretDto _rabbitMQSecretDto;
        private readonly IChannel _channel;
        #endregion

        public event Action<string>? OnMessageReceived;

        public RabbitConsumerService(RabbitMQSecretDto rabbitMQSecretDto)
        {
            _rabbitMQSecretDto = rabbitMQSecretDto;
            var factory = new ConnectionFactory() { HostName = _rabbitMQSecretDto.HostName!, UserName = _rabbitMQSecretDto.UserName!, Password = _rabbitMQSecretDto.Password! };
            var connection = factory.CreateConnectionAsync().Result;
            _channel = connection.CreateChannelAsync().Result;
            _channel.ExchangeDeclareAsync(
                exchange: _rabbitMQSecretDto.ExchangeName!, 
                type: _rabbitMQSecretDto.ExchangeType!, 
                durable: _rabbitMQSecretDto.ExchangeDurable, 
                autoDelete: _rabbitMQSecretDto.ExchangeAutoDelete).Wait();
            _channel.QueueDeclareAsync(
                queue: _rabbitMQSecretDto.QueueName!, 
                durable: _rabbitMQSecretDto.QueueDurable, 
                exclusive: _rabbitMQSecretDto.QueueExclusive, 
                autoDelete: _rabbitMQSecretDto.QueueAutoDelete, 
                arguments: _rabbitMQSecretDto.QueueArguments ?? null).Wait();
            _channel.QueueBindAsync(
                queue: _rabbitMQSecretDto.QueueName!, 
                exchange: _rabbitMQSecretDto.ExchangeName!, 
                routingKey: _rabbitMQSecretDto.RoutingKey!).Wait();
        }

        public void StartListening()
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var message = System.Text.Encoding.UTF8.GetString(ea.Body.Span);
                OnMessageReceived?.Invoke(message);
                await Task.CompletedTask;
            };
            _channel.BasicConsumeAsync(queue:_rabbitMQSecretDto.QueueName!, autoAck: _rabbitMQSecretDto.AutoAck, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
        }
    }
}