using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Procrastinator
{
    public enum ReminderTimelineType
    {
        Today,
        Tomorrow,
        ThisWeek,        
        NextWeek,        
        ThisMonth,       
        NextMonth,       
        LaterThisYear,  
        NextYear         
    }

    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        // Navigation property
        public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }

    // DTO for incoming POST requests to create reminders
    public class ReminderRequest
    {
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        [Required]
        public ReminderTimelineType ReminderTimeline { get; set; }
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
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
        
        public static DateTimeOffset GetReminderDateTime(ReminderTimelineType reminderTimeline)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            Random rand = new Random();
            switch (reminderTimeline)
            {
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
                    var nextYearEnd = new DateTimeOffset(nextYearStart.Year, 12, 31, 23, 59, 59, now.Offset);
                    var nextYearSpan = nextYearEnd - nextYearStart;
                    return nextYearStart.AddSeconds(rand.NextDouble() * nextYearSpan.TotalSeconds);

                default:
                    throw new ArgumentOutOfRangeException(nameof(reminderTimeline), "Invalid reminder timeline");
            }
        }
    }
}
