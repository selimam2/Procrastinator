namespace Procrastinator.Services
{
    public class AppConfiguration : IAppConfiguration
    {
        public int MaxRetries { get; }
        public int ServiceCheckIntervalMinutes { get; }
        public string DefaultTimeZone { get; }

        public AppConfiguration(IConfiguration configuration)
        {
            MaxRetries = configuration.GetValue<int>("AppSettings:MaxRetries", 3);
            ServiceCheckIntervalMinutes = configuration.GetValue<int>("AppSettings:ServiceCheckIntervalMinutes", 1);
            DefaultTimeZone = configuration.GetValue<string>("AppSettings:DefaultTimeZone", "UTC") ?? "UTC";
        }
    }
}
