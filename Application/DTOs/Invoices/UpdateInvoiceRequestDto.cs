using Domain.Enums;

namespace Application.DTOs.Invoices;

public class UpdateInvoiceRequestDto
{
    public Guid UserId { get; set; }

    public Guid ProjectId { get; set; }

    public decimal Amount { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal TaxAmount { get; set; }

    public PaymentStatus Status { get; set; }

    public string? Description { get; set; }

    public DateTime DueDate { get; set; }

    public DateTime? PaidAt { get; set; }
}