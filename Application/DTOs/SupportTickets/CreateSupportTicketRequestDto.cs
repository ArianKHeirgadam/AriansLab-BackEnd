using Domain.Enums;

namespace Application.DTOs.SupportTickets;

public class CreateSupportTicketRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public string? Attachment { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public long? FileSize { get; set; }
}