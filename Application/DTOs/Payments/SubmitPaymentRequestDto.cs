namespace Application.DTOs.Payments;

/// <summary>
/// Customer-supplied proof for a bank transfer. The payable amount and
/// gateway metadata are always calculated by the server.
/// </summary>
public sealed class SubmitPaymentRequestDto
{
    public Guid InvoiceId { get; set; }

    public string TrackingCode { get; set; } = string.Empty;

    public string? CardLastFour { get; set; }
}
