using Domain.Common;

namespace Domain.Entities
{
    public class ActivityLog : SoftDeleteEntity
    {
        public Guid UserId { get; set; }

        public string Activity { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? IpAddress { get; set; }

        public string? UserAgent { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
