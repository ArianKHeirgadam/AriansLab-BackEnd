namespace Application.DTOs.SupportTickets;

public class CreateTicketMessageRequestDto
{
    public string Message { get; set; } = string.Empty;

    public string? Attachment { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public long? FileSize { get; set; }
}