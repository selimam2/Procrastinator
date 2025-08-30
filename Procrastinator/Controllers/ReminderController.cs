using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Procrastinator.Models;

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
        public async Task<ActionResult<ReminderResponse>> CreateReminder([FromBody] ReminderRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Try to find existing user by contact info
            User? user = null;
            
            // First try to find by email
            user = await _context.EmailUsers
                .FirstOrDefaultAsync(u => u.EmailAddress == request.ContactInfo);
            
            // If not found by email, try by phone
            if (user == null)
            {
                user = await _context.PhoneUsers
                    .FirstOrDefaultAsync(u => u.PhoneNumber == request.ContactInfo);
            }

            if (user == null)
            {
                // Create user based on the contact info format
                if (IsValidEmail(request.ContactInfo))
                {
                    var emailUser = new EmailUser
                    {
                        EmailAddress = request.ContactInfo,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    _context.EmailUsers.Add(emailUser);
                    await _context.SaveChangesAsync();
                    user = emailUser;
                }
                else
                {
                    var phoneUser = new PhoneUser
                    {
                        PhoneNumber = request.ContactInfo,
                        CreatedAt = DateTimeOffset.UtcNow,
                        UpdatedAt = DateTimeOffset.UtcNow
                    };
                    _context.PhoneUsers.Add(phoneUser);
                    await _context.SaveChangesAsync();
                    user = phoneUser;
                }
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

            _logger.LogInformation($"Reminder created for {user.ContactInfo} at {reminder.ReminderDateTime}: {reminder.Message}");

            // Create response DTO
            var response = reminder.ToResponse();

            return CreatedAtAction(nameof(CreateReminder), new { id = reminder.Id }, response);
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
