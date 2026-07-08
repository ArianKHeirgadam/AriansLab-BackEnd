using Application.Common.Models;
using Application.DTOs.Notifications;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminNotificationsController : ControllerBase
{
    private readonly INotificationAdminService _notificationAdminService;

    public AdminNotificationsController(
        INotificationAdminService notificationAdminService)
    {
        _notificationAdminService = notificationAdminService;
    }

    /// <summary>
    /// Gets all notifications for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<NotificationDto>>.Ok(
            notifications,
            "Notifications retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets unread notifications for admin panel.
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<List<NotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<NotificationDto>>>> GetUnread(
        CancellationToken cancellationToken)
    {
        var notifications = await _notificationAdminService.GetUnreadAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<NotificationDto>>.Ok(
            notifications,
            "Unread notifications retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single notification by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationAdminService.GetByIdAsync(
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
    /// Creates a notification for a user.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Create(
        [FromBody] CreateNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _notificationAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = notification.Id },
                ApiResponse<NotificationDto>.Ok(
                    notification,
                    "Notification created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing notification.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateNotificationRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var notification = await _notificationAdminService.UpdateAsync(
                id,
                request,
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
                "Notification updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates notification read status.
    /// </summary>
    [HttpPatch("{id:guid}/read-status")]
    [ProducesResponseType(typeof(ApiResponse<NotificationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> UpdateReadStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateNotificationReadStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationAdminService.UpdateReadStatusAsync(
            id,
            request,
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
            "Notification read status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes a notification.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _notificationAdminService.DeleteAsync(
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
}