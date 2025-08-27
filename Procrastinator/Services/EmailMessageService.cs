namespace Procrastinator.Services
{
    public class EmailMessageService : IMessageService
    {
        private readonly ILogger<EmailMessageService> _logger;
        private readonly IConfiguration _configuration;

        public EmailMessageService(ILogger<EmailMessageService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public Task<bool> SendMessageAsync(string recipient, string message)
        {
            // Email sending is not implemented yet
            _logger.LogWarning($"Email sending not implemented. Would send to {recipient}: {message}");
            return Task.FromResult(false);
        }
    }
}
