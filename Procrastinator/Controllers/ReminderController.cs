using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Procrastinator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReminderController : ControllerBase
    {
        private readonly ProcrastinatorContext _context;
        private readonly ILogger<ReminderController> _logger;

        public ReminderController(ProcrastinatorContext context, ILogger<ReminderController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // POST: api/Reminder
        [HttpPost]
        public async Task<ActionResult<Reminder>> CreateReminder([FromBody] ReminderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Find or create user based on phone number
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (user == null)
            {
                // Create user if they don't exist
                user = new User
                {
                    PhoneNumber = request.PhoneNumber,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            var reminder = new Reminder
            {
                UserId = user.Id,
                Message = request.Message,
                ReminderDateTime = Reminder.GetReminderDateTime(request.ReminderTimeline),
                ReminderTimeline = request.ReminderTimeline.ToString(),
                IsCompleted = false,
                RetryCount = 0,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.Reminders.Add(reminder);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Reminder created for {reminder.UserId} at {reminder.ReminderDateTime}: {reminder.Message}");

            return CreatedAtAction(nameof(CreateReminder), new { id = reminder.Id }, reminder);
        }
    }
}
