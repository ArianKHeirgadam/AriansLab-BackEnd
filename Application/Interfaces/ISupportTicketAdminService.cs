using Application.DTOs.SupportTickets;

namespace Application.Interfaces;

public interface ISupportTicketAdminService
{
    Task<List<SupportTicketListItemDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> ReplyAsync(
        Guid adminUserId,
        Guid ticketId,
        CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> UpdateStatusAsync(
        Guid ticketId,
        UpdateSupportTicketStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> AssignAsync(
        Guid ticketId,
        AssignSupportTicketRequestDto request,
        CancellationToken cancellationToken = default);

    Task<SupportTicketDetailDto?> CloseAsync(
        Guid adminUserId,
        Guid ticketId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}