using Microsoft.EntityFrameworkCore;

namespace Procrastinator.Services
{
    public class ReminderDispatchService : BackgroundService
    {
        private readonly ILogger<ReminderDispatchService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppConfiguration _appConfig;

        public ReminderDispatchService(
            ILogger<ReminderDispatchService> logger, 
            IServiceProvider serviceProvider,
            IAppConfiguration appConfig)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _appConfig = appConfig;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var checkInterval = TimeSpan.FromMinutes(_appConfig.ServiceCheckIntervalMinutes);
            _logger.LogInformation("Reminder Dispatch Service started. Checking for due reminders every {interval} minutes.", _appConfig.ServiceCheckIntervalMinutes);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueRemindersAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while processing due reminders");
                }

                await Task.Delay(checkInterval, stoppingToken);
            }
        }

        private async Task ProcessDueRemindersAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ProcrastinatorContext>();
            var messageService = scope.ServiceProvider.GetRequiredService<IMessageService>();

            var now = DateTimeOffset.UtcNow;
            
            // Get all reminders that are due, not completed, and haven't exhausted retries
            var dueReminders = await context.Reminders
                .Include(r => r.User)
                .Where(r => !r.IsCompleted && 
                           r.RetryCount < _appConfig.MaxRetries &&
                           r.ReminderDateTime <= now)
                .ToListAsync();

            if (!dueReminders.Any())
            {
                _logger.LogDebug("No due reminders found at {time}", now);
                return;
            }

            _logger.LogInformation("Found {count} due reminders to process", dueReminders.Count);

            foreach (var reminder in dueReminders)
            {
                try
                {
                    var message = $"🔔 Reminder: {reminder.Message}";
                    
                    var success = await messageService.SendMessageAsync(reminder.User.PhoneNumber, message);
                    
                    if (success)
                    {
                        // Mark reminder as completed
                        reminder.IsCompleted = true;
                        reminder.UpdatedAt = DateTimeOffset.UtcNow;
                        
                        _logger.LogInformation("Successfully dispatched reminder {id} to {phoneNumber} after {retryCount} retries", 
                            reminder.Id, reminder.User.PhoneNumber, reminder.RetryCount);
                    }
                    else
                    {
                        // Increment retry count
                        reminder.RetryCount++;
                        reminder.UpdatedAt = now;
                        
                        if (reminder.RetryCount < _appConfig.MaxRetries)
                        {
                            _logger.LogWarning("Failed to dispatch reminder {id} to {phoneNumber}. Retry {retryCount}/{maxRetries}", 
                                reminder.Id, reminder.User.PhoneNumber, reminder.RetryCount, _appConfig.MaxRetries);
                        }
                        else
                        {
                            // Max retries exceeded
                            _logger.LogError("Reminder {id} to {phoneNumber} failed permanently after {retryCount} attempts", 
                                reminder.Id, reminder.User.PhoneNumber, reminder.RetryCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminder {id}", reminder.Id);
                    
                    // Handle unexpected errors with retry logic
                    reminder.RetryCount++;
                    reminder.UpdatedAt = now;
                    
                    if (reminder.RetryCount >= _appConfig.MaxRetries)
                    {
                        _logger.LogError("Reminder {id} failed permanently due to exception after {retryCount} attempts", 
                            reminder.Id, reminder.RetryCount);
                    }
                }
            }

            // Save all changes
            await context.SaveChangesAsync();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reminder Dispatch Service stopped");
            await base.StopAsync(cancellationToken);
        }
    }
}
