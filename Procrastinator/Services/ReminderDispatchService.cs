using Microsoft.EntityFrameworkCore;
using Procrastinator.Models;

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
            var emailService = scope.ServiceProvider.GetRequiredService<EmailMessageService>();
            var twilioService = scope.ServiceProvider.GetRequiredService<TwilioService>();

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
                    bool success = false;
                    
                    // Use appropriate service based on user type
                    if (reminder.User is EmailUser emailUser)
                    {
                        success = await emailService.SendMessageAsync(emailUser.EmailAddress, message);
                        _logger.LogInformation("Sending email reminder {id} to {email}", reminder.Id, emailUser.EmailAddress);
                    }
                    else if (reminder.User is PhoneUser phoneUser)
                    {
                        success = await twilioService.SendMessageAsync(phoneUser.PhoneNumber, message);
                        _logger.LogInformation("Sending SMS reminder {id} to {phoneNumber}", reminder.Id, phoneUser.PhoneNumber);
                    }
                    else
                    {
                        _logger.LogError("Unknown user type for reminder {id}", reminder.Id);
                        continue;
                    }
                    
                    if (success)
                    {
                        // Mark reminder as completed
                        reminder.IsCompleted = true;
                        reminder.UpdatedAt = DateTimeOffset.UtcNow;
                        
                        var contactInfo = reminder.User.ContactInfo;
                        _logger.LogInformation("Successfully dispatched reminder {id} to {contactInfo} after {retryCount} retries", 
                            reminder.Id, contactInfo, reminder.RetryCount);
                    }
                    else
                    {
                        // Increment retry count
                        reminder.RetryCount++;
                        reminder.UpdatedAt = DateTimeOffset.UtcNow;
                        
                        var contactInfo = reminder.User.ContactInfo;
                        if (reminder.RetryCount < _appConfig.MaxRetries)
                        {
                            _logger.LogWarning("Failed to dispatch reminder {id} to {contactInfo}. Retry {retryCount}/{maxRetries}", 
                                reminder.Id, contactInfo, reminder.RetryCount, _appConfig.MaxRetries);
                        }
                        else
                        {
                            // Max retries exceeded
                            _logger.LogError("Reminder {id} to {contactInfo} failed permanently after {retryCount} attempts", 
                                reminder.Id, contactInfo, reminder.RetryCount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing reminder {id}", reminder.Id);
                    
                    // Handle unexpected errors with retry logic
                    reminder.RetryCount++;
                    reminder.UpdatedAt = DateTimeOffset.UtcNow;
                    
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
