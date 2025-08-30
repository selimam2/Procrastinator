using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Procrastinator.Models
{
    // Base User class - abstract to enforce the inheritance pattern
    public abstract class User
    {
        [Key]
        public int Id { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        
        // Navigation property
        public virtual ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
        
        // Abstract property to get the contact information
        public abstract string ContactInfo { get; }
    }

    // Email User specialization
    public class EmailUser : User
    {
        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string EmailAddress { get; set; } = string.Empty;
        
        public override string ContactInfo => EmailAddress;
    }

    // Phone User specialization
    public class PhoneUser : User
    {
        [Required]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        
        public override string ContactInfo => PhoneNumber;
    }
}
