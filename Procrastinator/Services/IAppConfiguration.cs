namespace Procrastinator.Services
{
    public interface IAppConfiguration
    {
        int MaxRetries { get; }
        int ServiceCheckIntervalMinutes { get; }
        string DefaultTimeZone { get; }
    }
}
