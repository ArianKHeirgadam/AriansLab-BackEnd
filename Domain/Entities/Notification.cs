using Domain.Common;
using Domain.Enums;
namespace Domain.Entities
{
    public class Notification : SoftDeleteEntity
    {

        public Guid UserId { get; set; }
        public NotificationType Type { get; set; }
        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
