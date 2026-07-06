using Domain.Common;
using Domain.Enums;

namespace Domain.Entities
{
    public class Invoice : SoftDeleteEntity
    {
        public Guid UserId { get; set; }

        public Guid ProjectId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? Description { get; set; }

        public bool IsDeletedInvoice { get; set; }

        public bool IsPaid => Status == PaymentStatus.Paid;

        public DateTime? PaidAt { get; set; }

        public DateTime DueDate { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual Project Project { get; set; } = null!;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
