using Domain.Common;
using Domain.Enums;
namespace Domain.Entities
{
    public class Payment : SoftDeleteEntity
    {
        public Guid InvoiceId { get; set; }

        public decimal Amount { get; set; }

        public string Gateway { get; set; } = string.Empty;

        public string Authority { get; set; } = string.Empty;

        public string? RefId { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string? CardPan { get; set; }

        public string? TrackingCode { get; set; }

        public string? GatewayResponse { get; set; }

        public DateTime? PaidAt { get; set; }

        public virtual Invoice Invoice { get; set; } = null!;
    }
}
