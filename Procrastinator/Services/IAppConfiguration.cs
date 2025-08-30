namespace Procrastinator.Services
{
    public interface IAppConfiguration
    {
        int MaxRetries { get; }
        int ServiceCheckIntervalMinutes { get; }
        string DefaultTimeZone { get; }
        string TwilioAccountSid { get; }
        string TwilioAuthToken { get; }
        string TwilioFromPhoneNumber { get; }
        string EmailSmtpServer { get; }
        int EmailSmtpPort { get; }
        string EmailUsername { get; }
        string EmailPassword { get; }
        string EmailFromEmail { get; }
        string EmailFromName { get; }
        bool EmailEnableSsl { get; }
        bool EmailUseDefaultCredentials { get; }
        int EmailTimeoutSeconds { get; }
    }
}
