using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Procrastinator.Models;

namespace Procrastinator.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ProcrastinatorContext _context;

        public UserController(ProcrastinatorContext context)
        {
            _context = context;
        }

        // POST: api/User/Email
        [HttpPost("Email")]
        public async Task<ActionResult<EmailUser>> CreateEmailUser([FromBody] CreateEmailUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user with this email already exists
            var existingUser = await _context.EmailUsers
                .FirstOrDefaultAsync(u => u.EmailAddress == request.EmailAddress);

            if (existingUser != null)
            {
                return Conflict($"User with email {request.EmailAddress} already exists.");
            }

            var user = new EmailUser
            {
                EmailAddress = request.EmailAddress,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.EmailUsers.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreateEmailUser), new { id = user.Id }, user);
        }

        // POST: api/User/Phone
        [HttpPost("Phone")]
        public async Task<ActionResult<PhoneUser>> CreatePhoneUser([FromBody] CreatePhoneUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user with this phone number already exists
            var existingUser = await _context.PhoneUsers
                .FirstOrDefaultAsync(u => u.PhoneNumber == request.PhoneNumber);

            if (existingUser != null)
            {
                return Conflict($"User with phone number {request.PhoneNumber} already exists.");
            }

            var user = new PhoneUser
            {
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            _context.PhoneUsers.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(CreatePhoneUser), new { id = user.Id }, user);
        }
    }

    public class CreateEmailUserRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string EmailAddress { get; set; } = string.Empty;
    }

    public class CreatePhoneUserRequest
    {
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
