using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Procrastinator.Services
{
    public class TwilioService : IMessageService
    {
        private readonly ILogger<TwilioService> _logger;
        private readonly IConfiguration _configuration;

        public TwilioService(ILogger<TwilioService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<bool> SendMessageAsync(string recipient, string message)
        {
            try
            {
                var accountSid = _configuration["Twilio:AccountSid"];
                var authToken = _configuration["Twilio:AuthToken"];
                var fromPhoneNumber = _configuration["Twilio:FromPhoneNumber"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(fromPhoneNumber))
                {
                    _logger.LogError("Twilio configuration is missing. Please check appsettings.json");
                    return false;
                }

                TwilioClient.Init(accountSid, authToken);

                var messageResource = await MessageResource.CreateAsync(
                    to: new PhoneNumber(recipient),
                    from: new PhoneNumber(fromPhoneNumber),
                    body: message
                );

                _logger.LogInformation($"SMS sent successfully to {recipient}. SID: {messageResource.Sid}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send SMS to {recipient}: {ex.Message}");
                return false;
            }
        }
    }
}
