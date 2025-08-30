using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Procrastinator.Models
{
    public enum ReminderTimelineType
    {
        Imminent,       // New timeline for next 5 minutes
        Today,
        Tomorrow,
        ThisWeek,        
        NextWeek,        
        ThisMonth,       
        NextMonth,       
        LaterThisYear,  
        NextYear         
    }

    // DTO for incoming POST requests to create reminders
    public class ReminderRequest
    {
        [Required]
        public string ContactInfo { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public ReminderTimelineType ReminderTimeline { get; set; }
    }

    // DTO for API responses to avoid circular references
    public class ReminderResponse
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTimeOffset ReminderDateTime { get; set; }
        public string ReminderTimeline { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public int RetryCount { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public string ContactInfo { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
    }

    // Internal model for storing reminders in the database
    public class Reminder
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public DateTimeOffset ReminderDateTime { get; set; }
        
        [Required]
        [StringLength(20)]
        public string ReminderTimeline { get; set; } = string.Empty;
        
        public bool IsCompleted { get; set; }
        
        // Simple retry tracking
        public int RetryCount { get; set; } = 0;
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        // Navigation property - can be either EmailUser or PhoneUser
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        // Helper method to convert to DTO
        public ReminderResponse ToResponse()
        {
            return new ReminderResponse
            {
                Id = Id,
                Message = Message,
                ReminderDateTime = ReminderDateTime,
                ReminderTimeline = ReminderTimeline,
                IsCompleted = IsCompleted,
                RetryCount = RetryCount,
                CreatedAt = CreatedAt,
                UpdatedAt = UpdatedAt,
                ContactInfo = User.ContactInfo,
                UserType = User is EmailUser ? "Email" : "Phone"
            };
        }
        
        public static DateTimeOffset GetReminderDateTime(ReminderTimelineType reminderTimeline)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            Random rand = new Random();
            switch (reminderTimeline)
            {
                case ReminderTimelineType.Imminent:
                    // Send reminders within the next 5 minutes
                    return now.AddMinutes(rand.Next(0, 5));

                case ReminderTimelineType.Today:
                    // Random time today between now and midnight
                    var todayEnd = now.Date.AddDays(1).AddSeconds(-1);
                    var todaySpan = todayEnd - now;
                    return now.AddSeconds(rand.NextDouble() * todaySpan.TotalSeconds);

                case ReminderTimelineType.Tomorrow:
                    // Random time tomorrow
                    var tomorrowStart = now.Date.AddDays(1);
                    var tomorrowEnd = tomorrowStart.AddDays(1).AddSeconds(-1);
                    var tomorrowSpan = tomorrowEnd - tomorrowStart;
                    return tomorrowStart.AddSeconds(rand.NextDouble() * tomorrowSpan.TotalSeconds);

                case ReminderTimelineType.ThisWeek:
                    // Random time between now and end of week (Sunday)
                    var weekEnd = now.Date.AddDays(7 - (int)now.DayOfWeek).AddSeconds(-1);
                    var weekSpan = weekEnd - now;
                    return now.AddSeconds(rand.NextDouble() * weekSpan.TotalSeconds);

                case ReminderTimelineType.NextWeek:
                    // Random time next week (Monday to Sunday)
                    var nextWeekStart = now.Date.AddDays(7 - (int)now.DayOfWeek);
                    var nextWeekEnd = nextWeekStart.AddDays(7).AddSeconds(-1);
                    var nextWeekSpan = nextWeekEnd - nextWeekStart;
                    return nextWeekStart.AddSeconds(rand.NextDouble() * nextWeekSpan.TotalSeconds);

                case ReminderTimelineType.ThisMonth:
                    // Random time between now and end of month
                    var thisMonthEnd = new DateTimeOffset(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month), 23, 59, 59, now.Offset);
                    var thisMonthSpan = thisMonthEnd - now;
                    return now.AddSeconds(rand.NextDouble() * thisMonthSpan.TotalSeconds);

                case ReminderTimelineType.NextMonth:
                    // Random time next month
                    var nextMonth = now.Month == 12 ? 1 : now.Month + 1;
                    var nextMonthYear = now.Month == 12 ? now.Year + 1 : now.Year;
                    var nextMonthStart = new DateTimeOffset(nextMonthYear, nextMonth, 1, 0, 0, 0, now.Offset);
                    var nextMonthEnd = new DateTimeOffset(nextMonthYear, nextMonth, DateTime.DaysInMonth(nextMonthYear, nextMonth), 23, 59, 59, now.Offset);
                    var nextMonthSpan = nextMonthEnd - nextMonthStart;
                    return nextMonthStart.AddSeconds(rand.NextDouble() * nextMonthSpan.TotalSeconds);

                case ReminderTimelineType.LaterThisYear:
                    // Random time between now and end of year
                    var yearEnd = new DateTimeOffset(now.Year, 12, 31, 23, 59, 59, now.Offset);
                    var yearSpan = yearEnd - now;
                    return now.AddSeconds(rand.NextDouble() * yearSpan.TotalSeconds);

                case ReminderTimelineType.NextYear:
                    // Random time next year
                    var nextYearStart = new DateTimeOffset(now.Year + 1, 1, 1, 0, 0, 0, now.Offset);
                    var nextYearEnd = new DateTimeOffset(nextYearStart.Year, 12, 31, 23, 59, 59, nextYearStart.Offset);
                    var nextYearSpan = nextYearEnd - nextYearStart;
                    return nextYearStart.AddSeconds(rand.NextDouble() * nextYearSpan.TotalSeconds);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reminderTimeline), "Invalid reminder timeline");
            }
        }
    }
}
