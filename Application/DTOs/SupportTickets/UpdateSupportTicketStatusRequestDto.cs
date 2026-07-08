using Domain.Enums;

namespace Application.DTOs.SupportTickets;

public class UpdateSupportTicketStatusRequestDto
{
    public TicketStatus Status { get; set; }

    public TicketPriority Priority { get; set; }

    public Guid? AssignedToUserId { get; set; }
}