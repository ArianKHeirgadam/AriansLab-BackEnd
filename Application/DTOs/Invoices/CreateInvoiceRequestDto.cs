using Domain.Enums;

namespace Application.DTOs.Invoices;

public class CreateInvoiceRequestDto
{
    public Guid UserId { get; set; }

    public Guid ProjectId { get; set; }

    public string? InvoiceNumber { get; set; }

    public decimal Amount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }
}