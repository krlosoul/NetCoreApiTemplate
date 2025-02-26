namespace Application.Interfaces.Services
{
    public interface IKafkaConsumerService : IDisposable
    {            
        /// <summary>
        /// Event that is triggered when a message is received from RabbitMQ.
        /// </summary>
        public event Action<string> OnMessageReceived;

        /// <summary>
        /// Starts listening to RabbitMQ messages.
        /// </summary>
        public void StartListening();
    }
}