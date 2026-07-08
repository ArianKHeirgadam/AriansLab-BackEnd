using Domain.Enums;

namespace Application.DTOs.Invoices;

public class InvoicePaymentDto
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Gateway { get; set; } = string.Empty;

    public string Authority { get; set; } = string.Empty;

    public string? RefId { get; set; }

    public PaymentStatus Status { get; set; }

    public string? TrackingCode { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}