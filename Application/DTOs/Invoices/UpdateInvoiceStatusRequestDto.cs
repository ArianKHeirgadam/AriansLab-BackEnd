using Domain.Enums;

namespace Application.DTOs.Invoices;

public class UpdateInvoiceStatusRequestDto
{
    public PaymentStatus Status { get; set; }
}