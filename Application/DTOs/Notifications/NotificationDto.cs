using Domain.Enums;

namespace Application.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string UserFullName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}