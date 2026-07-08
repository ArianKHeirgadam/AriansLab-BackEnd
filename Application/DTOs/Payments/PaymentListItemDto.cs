using Domain.Enums;

namespace Application.DTOs.Payments;

public class PaymentListItemDto
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Gateway { get; set; } = string.Empty;

    public string Authority { get; set; } = string.Empty;

    public string? RefId { get; set; }

    public PaymentStatus Status { get; set; }

    public string? TrackingCode { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}