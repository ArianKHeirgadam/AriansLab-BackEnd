using Domain.Common;
namespace Domain.Entities
{
    public class TicketMessage : SoftDeleteEntity
    {
        public Guid TicketId { get; set; }

        public Guid SenderId { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? Attachment { get; set; }
        public string? FileName { get; set; }

        public string? FilePath { get; set; }
        public bool IsRead { get; set; }

        public long? FileSize { get; set; }

        public bool IsAdminMessage { get; set; }

        public virtual SupportTicket Ticket { get; set; } = null!;

        public virtual User Sender { get; set; } = null!;
    }
}
