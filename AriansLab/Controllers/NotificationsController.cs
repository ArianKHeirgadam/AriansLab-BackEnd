using Application.Common.Models;
using Application.DTOs.Notifications;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationsController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Gets notifications that belong to the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyNotifications(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var notifications = await _notificationService.GetMyNotificationsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<NotificationDto>>.Ok(
            notifications,
            "Notifications retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets unread notifications that belong to the authenticated user.
    /// </summary>
    [HttpGet("my/unread")]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetMyUnreadNotifications(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var notifications = await _notificationService.GetMyUnreadNotificationsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<NotificationDto>>.Ok(
            notifications,
            "Unread notifications retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single notification that belongs to the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetMyNotificationById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var notification = await _notificationService.GetMyNotificationByIdAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (notification is null)
        {
            return NotFound(ApiResponse.Fail(
                "Notification was not found."
            ));
        }

        return Ok(ApiResponse<NotificationDto>.Ok(
            notification,
            "Notification retrieved successfully."
        ));
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    [HttpPatch("my/{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var notification = await _notificationService.MarkAsReadAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (notification is null)
        {
            return NotFound(ApiResponse.Fail(
                "Notification was not found."
            ));
        }

        return Ok(ApiResponse<NotificationDto>.Ok(
            notification,
            "Notification marked as read successfully."
        ));
    }

    /// <summary>
    /// Marks all authenticated user's notifications as read.
    /// </summary>
    [HttpPatch("my/read-all")]
    [ProducesResponseType(typeof(ApiResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var updatedCount = await _notificationService.MarkAllAsReadAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<int>.Ok(
            updatedCount,
            "Notifications marked as read successfully."
        ));
    }

    /// <summary>
    /// Deletes one of the authenticated user's notifications.
    /// </summary>
    [HttpDelete("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> DeleteMyNotification(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var deleted = await _notificationService.DeleteMyNotificationAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Notification was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Notification deleted successfully."
        ));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }
}