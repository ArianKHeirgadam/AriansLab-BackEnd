using Application.Common.Models;
using Application.DTOs.ContactMessages;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/contact-messages")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminContactMessagesController : ControllerBase
{
    private readonly IContactMessageAdminService _contactMessageAdminService;

    public AdminContactMessagesController(
        IContactMessageAdminService contactMessageAdminService)
    {
        _contactMessageAdminService = contactMessageAdminService;
    }

    /// <summary>
    /// Gets all contact messages for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AdminContactMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminContactMessageDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var messages = await _contactMessageAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<AdminContactMessageDto>>.Ok(
            messages,
            "Contact messages retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets unread contact messages for admin panel.
    /// </summary>
    [HttpGet("unread")]
    [ProducesResponseType(typeof(ApiResponse<List<AdminContactMessageDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminContactMessageDto>>>> GetUnread(
        CancellationToken cancellationToken)
    {
        var messages = await _contactMessageAdminService.GetUnreadAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<AdminContactMessageDto>>.Ok(
            messages,
            "Unread contact messages retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single contact message by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminContactMessageDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var message = await _contactMessageAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (message is null)
        {
            return NotFound(ApiResponse.Fail(
                "Contact message was not found."
            ));
        }

        return Ok(ApiResponse<AdminContactMessageDto>.Ok(
            message,
            "Contact message retrieved successfully."
        ));
    }

    /// <summary>
    /// Marks a contact message as read.
    /// </summary>
    [HttpPatch("{id:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<AdminContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminContactMessageDto>>> MarkAsRead(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var message = await _contactMessageAdminService.MarkAsReadAsync(
            id,
            cancellationToken
        );

        if (message is null)
        {
            return NotFound(ApiResponse.Fail(
                "Contact message was not found."
            ));
        }

        return Ok(ApiResponse<AdminContactMessageDto>.Ok(
            message,
            "Contact message marked as read successfully."
        ));
    }

    /// <summary>
    /// Saves an admin reply for a contact message.
    /// </summary>
    [HttpPost("{id:guid}/reply")]
    [ProducesResponseType(typeof(ApiResponse<AdminContactMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminContactMessageDto>>> Reply(
        [FromRoute] Guid id,
        [FromBody] ReplyContactMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var message = await _contactMessageAdminService.ReplyAsync(
                id,
                request,
                cancellationToken
            );

            if (message is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Contact message was not found."
                ));
            }

            return Ok(ApiResponse<AdminContactMessageDto>.Ok(
                message,
                "Reply saved successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a contact message.
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
        var deleted = await _contactMessageAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Contact message was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Contact message deleted successfully."
        ));
    }
}