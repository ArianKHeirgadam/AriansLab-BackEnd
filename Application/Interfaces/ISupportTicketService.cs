using Application.DTOs.SupportTickets;

namespace Application.Interfaces;

public interface ISupportTicketService
{
    Task<List<SupportTicketListItemDto>> GetMyTicketsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> GetMyTicketByIdAsync(
        Guid userId,
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto> CreateAsync(
        Guid userId,
        CreateSupportTicketRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> AddMessageAsync(
        Guid userId,
        Guid ticketId,
        CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> CloseMyTicketAsync(
        Guid userId,
        Guid ticketId,
        CancellationToken cancellationToken = default);
}