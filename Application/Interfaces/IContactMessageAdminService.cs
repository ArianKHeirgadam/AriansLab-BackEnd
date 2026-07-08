using Application.DTOs.ContactMessages;

namespace Application.Interfaces;

public interface IContactMessageAdminService
{
    Task<List<AdminContactMessageDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<AdminContactMessageDto>> GetUnreadAsync(
        CancellationToken cancellationToken = default);

    Task<AdminContactMessageDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminContactMessageDto?> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminContactMessageDto?> MarkAsRepliedAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}