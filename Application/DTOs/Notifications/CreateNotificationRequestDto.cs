using Domain.Enums;

namespace Application.DTOs.Notifications;

public class CreateNotificationRequestDto
{
    public Guid UserId { get; set; }

    public NotificationType Type { get; set; } = NotificationType.Info;

    public string Title { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;
}