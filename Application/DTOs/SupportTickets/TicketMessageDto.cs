using Domain.Enums;

namespace Application.DTOs.SupportTickets;

public class TicketMessageDto
{
    public Guid Id { get; set; }

    public Guid TicketId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderFullName { get; set; } = string.Empty;

    public string SenderEmail { get; set; } = string.Empty;

    public UserRole SenderRole { get; set; }

    public string Message { get; set; } = string.Empty;

    public string? Attachment { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public long? FileSize { get; set; }

    public bool IsRead { get; set; }

    public bool IsAdminMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}