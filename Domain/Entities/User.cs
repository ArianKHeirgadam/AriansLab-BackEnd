using Domain.Enums;
using Microsoft.AspNetCore.Identity;
namespace Domain.Entities
{
    public class User: IdentityUser<Guid>
    {
        public string FullName { get; set; } = string.Empty;

        public UserRole Role { get; set; }

        public bool IsActive { get; set; } = true;
        public string? Avatar { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public ICollection<Project> Projects { get; set; } = new List<Project>();

        public ICollection<SupportTicket> SupportTickets { get; set; } = new List<SupportTicket>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    }
}
