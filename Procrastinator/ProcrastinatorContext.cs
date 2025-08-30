using Microsoft.EntityFrameworkCore;
using Procrastinator.Models;

namespace Procrastinator
{
    public class ProcrastinatorContext : DbContext
    {
        public ProcrastinatorContext(DbContextOptions<ProcrastinatorContext> options)
            : base(options)
        {
        }

        // Entity sets for the inheritance hierarchy
        public DbSet<EmailUser> EmailUsers { get; set; }
        public DbSet<PhoneUser> PhoneUsers { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure User inheritance hierarchy
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure Table-Per-Hierarchy (TPH) inheritance
                entity.HasDiscriminator<string>("UserType")
                      .HasValue<EmailUser>("Email")
                      .HasValue<PhoneUser>("Phone");
            });

            // Configure EmailUser specific properties
            modelBuilder.Entity<EmailUser>(entity =>
            {
                entity.Property(e => e.EmailAddress).IsRequired().HasMaxLength(255);
                entity.HasIndex(e => e.EmailAddress).IsUnique();
                
                // Add check constraint for EmailUser
                entity.ToTable("User", t => t
                    .HasCheckConstraint("CK_User_EmailUser_EmailNotNull", 
                        "(\"UserType\" != 'Email') OR (\"EmailAddress\" IS NOT NULL)"));
            });

            // Configure PhoneUser specific properties
            modelBuilder.Entity<PhoneUser>(entity =>
            {
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.PhoneNumber).IsUnique();
                
                // Add check constraint for PhoneUser
                entity.ToTable("User", t => t
                    .HasCheckConstraint("CK_User_PhoneUser_PhoneNotNull", 
                        "(\"UserType\" != 'Phone') OR (\"PhoneNumber\" IS NOT NULL)"));
            });

            // Configure Reminder entity
            modelBuilder.Entity<Reminder>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired();
                entity.Property(e => e.ReminderDateTime).IsRequired();
                entity.Property(e => e.ReminderTimeline).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsCompleted).HasDefaultValue(false);
                entity.Property(e => e.RetryCount).HasDefaultValue(0);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Configure relationship with the base User class
                entity.HasOne(e => e.User)
                      .WithMany(u => u.Reminders)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
