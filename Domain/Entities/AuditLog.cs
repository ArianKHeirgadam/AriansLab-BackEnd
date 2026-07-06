using Domain.Common;

namespace Domain.Entities
{
    public class AuditLog : SoftDeleteEntity
    {
        public Guid? UserId { get; set; }

        public string Action { get; set; } = string.Empty;

        public string EntityName { get; set; } = string.Empty;

        public string EntityId { get; set; } = string.Empty;

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public virtual User? User { get; set; }
    }
}
