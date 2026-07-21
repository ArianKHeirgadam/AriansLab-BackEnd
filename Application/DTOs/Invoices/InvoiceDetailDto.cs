using Domain.Enums;

namespace Application.DTOs.Invoices;

public class InvoiceDetailDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CustomerFullName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectTitle { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public decimal FinalAmount { get; set; }

    public PaymentStatus Status { get; set; }

    public bool IsPaid { get; set; }

    public bool IsFinalized { get; set; }

    public bool HasPendingPayment { get; set; }

    public decimal PaidAmount { get; set; }

    public decimal RemainingAmount { get; set; }

    public string? Description { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<InvoicePaymentDto> Payments { get; set; } =
        Array.Empty<InvoicePaymentDto>();
}
