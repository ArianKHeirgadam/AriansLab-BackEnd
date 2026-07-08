using Domain.Enums;

namespace Application.DTOs.SupportTickets;

public class SupportTicketDetailDto
{
    public Guid Id { get; set; }

    public string TicketNumber { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string CustomerFullName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public Guid? AssignedToUserId { get; set; }

    public string? AssignedToFullName { get; set; }

    public Guid? ClosedByUserId { get; set; }

    public string? ClosedByFullName { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<TicketMessageDto> Messages { get; set; } =
        Array.Empty<TicketMessageDto>();
}