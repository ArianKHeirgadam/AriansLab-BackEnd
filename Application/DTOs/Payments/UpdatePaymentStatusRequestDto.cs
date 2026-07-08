using Domain.Enums;

namespace Application.DTOs.Payments;

public class UpdatePaymentStatusRequestDto
{
    public PaymentStatus Status { get; set; }

    public string? RefId { get; set; }

    public string? CardPan { get; set; }

    public string? TrackingCode { get; set; }

    public string? GatewayResponse { get; set; }

    public DateTime? PaidAt { get; set; }
}