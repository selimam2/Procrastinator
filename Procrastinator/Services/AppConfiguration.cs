namespace Procrastinator.Services
{
    public class AppConfiguration : IAppConfiguration
    {
        public int MaxRetries { get; }
        public int ServiceCheckIntervalMinutes { get; }
        public string DefaultTimeZone { get; }
        public string TwilioAccountSid { get; }
        public string TwilioAuthToken { get; }
        public string TwilioFromPhoneNumber { get; }
        public string EmailSmtpServer { get; }
        public int EmailSmtpPort { get; }
        public string EmailUsername { get; }
        public string EmailPassword { get; }
        public string EmailFromEmail { get; }
        public string EmailFromName { get; }
        public bool EmailEnableSsl { get; }
        public bool EmailUseDefaultCredentials { get; }
        public int EmailTimeoutSeconds { get; }

        public AppConfiguration(IConfiguration configuration)
        {
            MaxRetries = configuration.GetValue<int>("AppSettings:MaxRetries", 3);
            ServiceCheckIntervalMinutes = configuration.GetValue<int>("AppSettings:ServiceCheckIntervalMinutes", 1);
            DefaultTimeZone = configuration.GetValue<string>("AppSettings:DefaultTimeZone", "UTC") ?? "UTC";
            TwilioAccountSid = configuration["Twilio:AccountSid"] ?? string.Empty;
            TwilioAuthToken = configuration["Twilio:AuthToken"] ?? string.Empty;
            TwilioFromPhoneNumber = configuration["Twilio:FromPhoneNumber"] ?? string.Empty;
            
            // Email configuration
            EmailSmtpServer = configuration["Email:SmtpServer"] ?? string.Empty;
            EmailSmtpPort = configuration.GetValue<int>("Email:SmtpPort", 587);
            EmailUsername = configuration["Email:Username"] ?? string.Empty;
            EmailPassword = configuration["Email:Password"] ?? string.Empty;
            EmailFromEmail = configuration["Email:FromEmail"] ?? string.Empty;
            EmailFromName = configuration["Email:FromName"] ?? "Procrastinator";
            EmailEnableSsl = configuration.GetValue<bool>("Email:EnableSsl", true);
            EmailUseDefaultCredentials = configuration.GetValue<bool>("Email:UseDefaultCredentials", false);
            EmailTimeoutSeconds = configuration.GetValue<int>("Email:TimeoutSeconds", 30);
        }
    }
}
