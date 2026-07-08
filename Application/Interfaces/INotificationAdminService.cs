using Application.DTOs.Notifications;

namespace Application.Interfaces;

public interface INotificationAdminService
{
    Task<List<NotificationDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<NotificationDto>> GetUnreadAsync(
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<NotificationDto> CreateAsync(
        CreateNotificationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> UpdateAsync(
        Guid id,
        UpdateNotificationRequestDto request,
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> UpdateReadStatusAsync(
        Guid id,
        UpdateNotificationReadStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}