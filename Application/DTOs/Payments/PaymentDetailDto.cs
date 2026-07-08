using Domain.Enums;

namespace Application.DTOs.Payments;

public class PaymentDetailDto
{
    public Guid Id { get; set; }

    public Guid InvoiceId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string CustomerFullName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public string ProjectTitle { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Gateway { get; set; } = string.Empty;

    public string Authority { get; set; } = string.Empty;

    public string? RefId { get; set; }

    public PaymentStatus Status { get; set; }

    public string? CardPan { get; set; }

    public string? TrackingCode { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}