using Domain.Common;
using Domain.Enums;
namespace Domain.Entities
{
    public class SupportTicket : SoftDeleteEntity
    {
        public string TicketNumber { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketStatus Status { get; set; } = TicketStatus.Open;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;

        public Guid? AssignedToUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual User? AssignedToUser { get; set; }

        public virtual User? ClosedByUser { get; set; }

        public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    }
}
