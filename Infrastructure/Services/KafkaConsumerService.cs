namespace Infrastructure.Services
{
    using System.Threading;
    using Application.Interfaces.Services;
    using Confluent.Kafka;
    using Core.Dtos.SecretsDto;

    public class KafkaConsumerService : IKafkaConsumerService
    {
        #region Properties 
        private readonly KafkaSecretDto _kafkaSecretDto;
        private readonly IConsumer<Ignore, string> _consumer;
        private readonly CancellationTokenSource _cts = new();
        private Task? _consumerTask;
        private bool _disposed = false;
        #endregion

        public event Action<string>? OnMessageReceived;

        public KafkaConsumerService(KafkaSecretDto kafkaSecretDto)
        {
            _kafkaSecretDto = kafkaSecretDto;

            var config = new ConsumerConfig
            {
                BootstrapServers = _kafkaSecretDto.BootstrapServers,
                GroupId = _kafkaSecretDto.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            _consumer.Subscribe(_kafkaSecretDto.Topic);
        }

        public void StartListening()
        {
            _consumerTask = Task.Run(() =>
            {
                try
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        var consumeResult = _consumer.Consume(_cts.Token);
                        if (consumeResult != null)
                        {
                            var message = consumeResult.Message.Value;
                            OnMessageReceived?.Invoke(message);
                        }
                    }
                }
                catch (OperationCanceledException){ }
                finally
                {
                    _consumer.Close();
                }
            });
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }

            if (_consumerTask != null)
            {
                try
                {
                    _consumerTask.Wait();
                }
                catch (AggregateException ex) when (ex.InnerExceptions.All(e => e is OperationCanceledException)) { }
            }

            _cts.Dispose();

            try
            {
                _consumer.Dispose();
            }
            catch (ObjectDisposedException) { }
        }
    }
}