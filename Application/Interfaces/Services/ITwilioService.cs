namespace Application.Interfaces.Services
{
    public interface ITwilioService
    {
        /// <summary>
        /// Send message to WhatsApp.
        /// </summary>
        /// <param name="to">The Destination.</param>
        /// <param name="message">The Message.</param>
        /// <returns></returns>
        Task SendWhatsAppAsync(string to, string message);

        /// <summary>
        /// Send message to WhatsApp with template.
        /// </summary>
        /// <param name="to">The Destination.</param>
        /// <param name="entity">The Data.</param>
        /// <returns></returns>
        Task SendWhatsAppThemeAsync<T>(string to, T entity);
    }
}