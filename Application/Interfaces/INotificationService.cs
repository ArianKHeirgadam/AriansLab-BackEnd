using Application.DTOs.Notifications;

namespace Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMyNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<List<NotificationDto>> GetMyUnreadNotificationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> GetMyNotificationByIdAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<NotificationDto?> MarkAsReadAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);

    Task<int> MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteMyNotificationAsync(
        Guid userId,
        Guid notificationId,
        CancellationToken cancellationToken = default);
}