namespace Infrastructure.Services
{  
    using System.Text.Json;
    using System.Threading.Tasks;
    using Application.Interfaces.Services;
    using Twilio;
    using Twilio.Exceptions;
    using Twilio.Rest.Api.V2010.Account;
    using Twilio.Types;
    using Core.Dtos.SecretsDto;
    using Core.Extensions;

    public class TwilioService : ITwilioService
    {
        #region Properties
        private readonly TwilioSecretDto? _twilioSecretDto;
        #endregion

        public TwilioService(TwilioSecretDto twilioSecretDto)
        {
            _twilioSecretDto = twilioSecretDto;
            TwilioClient.Init(_twilioSecretDto.AccountSID, _twilioSecretDto.AuthToken);
        }       
        
        public async Task SendWhatsAppAsync(string to, string message)
        {
            try
            {
                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber($"whatsapp:{to}"),
                    from: new PhoneNumber($"whatsapp:{_twilioSecretDto?.PhoneNumber}"),
                    body: message
                );

                if (messageResource.Status == MessageResource.StatusEnum.Failed ||
                    messageResource.Status == MessageResource.StatusEnum.Undelivered)
                {
                    throw new Exception($"Mensaje no enviado. Estado: {messageResource.Status}. Error: {messageResource.ErrorMessage}");
                }

                Console.WriteLine($"Mensaje enviado correctamente. SID: {messageResource.Sid}, Estado: {messageResource.Status}");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Error API de Twilio: Código {ex.Code}, Mensaje: {ex.Message}");
                throw new Exception($"Twilio API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                throw;
            }
        }
    
        public async Task SendWhatsAppThemeAsync<T>(string to, T entity)
        {
            try
            {
                var messageOptions = new CreateMessageOptions(
                new PhoneNumber($"whatsapp:{to}"));
                messageOptions.From = new PhoneNumber($"whatsapp:{_twilioSecretDto?.PhoneNumber}");
                messageOptions.ContentSid = _twilioSecretDto?.ContentSid;
                var parameters = entity.GetParameters();
                messageOptions.ContentVariables = JsonSerializer.Serialize(parameters);

                var message = await MessageResource.CreateAsync(messageOptions);

                if (message.Status == MessageResource.StatusEnum.Failed ||
                    message.Status == MessageResource.StatusEnum.Undelivered)
                {
                    throw new Exception($"Mensaje no enviado. Estado: {message.Status}. Error: {message.ErrorMessage}");
                }

                Console.WriteLine($"Mensaje enviado correctamente. SID: {message.Sid}, Estado: {message.Status}");
            }
            catch (ApiException ex)
            {
                Console.WriteLine($"Error API de Twilio: Código {ex.Code}, Mensaje: {ex.Message}");
                throw new Exception($"Twilio API Error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar mensaje: {ex.Message}");
                throw;
            }
        }
    }
}