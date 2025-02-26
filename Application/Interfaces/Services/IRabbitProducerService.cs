namespace Application.Interfaces.Services
{
    public interface IRabbitProducerService
    {
        /// <summary>
        /// Send a messagge to kafka.
        /// </summary>
        /// <param name="message">The message.</param>
        public Task SendMessageAsync(string message);
    }   
}