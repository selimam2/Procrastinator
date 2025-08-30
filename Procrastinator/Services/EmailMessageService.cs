using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace Procrastinator.Services
{
    public class EmailMessageService : IMessageService
    {
        private readonly ILogger<EmailMessageService> _logger;
        private readonly IAppConfiguration _appConfiguration;

        public EmailMessageService(ILogger<EmailMessageService> logger, IAppConfiguration appConfiguration)
        {
            _logger = logger;
            _appConfiguration = appConfiguration;
        }

        public async Task<bool> SendMessageAsync(string recipient, string message)
        {
            try
            {
                // Check if email configuration exists
                if (string.IsNullOrEmpty(_appConfiguration.EmailSmtpServer))
                {
                    _logger.LogWarning("Email configuration not found. Please configure Email section in appsettings.json");
                    return false;
                }

                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(
                    _appConfiguration.EmailFromName, 
                    _appConfiguration.EmailFromEmail));
                emailMessage.To.Add(new MailboxAddress("", recipient));
                emailMessage.Subject = "Procrastinator Reminder";
                emailMessage.Body = new TextPart(TextFormat.Plain) { Text = message };

                using var client = new SmtpClient();
                
                // Configure SMTP client
                await client.ConnectAsync(
                    _appConfiguration.EmailSmtpServer,
                    _appConfiguration.EmailSmtpPort,
                    SecureSocketOptions.StartTls);

                // Authenticate if credentials are provided
                if (!string.IsNullOrEmpty(_appConfiguration.EmailUsername) && !string.IsNullOrEmpty(_appConfiguration.EmailPassword))
                {
                    await client.AuthenticateAsync(_appConfiguration.EmailUsername, _appConfiguration.EmailPassword);
                }

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Recipient}", recipient);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipient}: {Message}", recipient, ex.Message);
                return false;
            }
        }
    }
}
