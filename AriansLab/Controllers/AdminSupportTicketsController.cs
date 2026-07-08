using Application.Common.Models;
using Application.DTOs.SupportTickets;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/support-tickets")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSupportTicketsController : ControllerBase
{
    private readonly ISupportTicketAdminService _supportTicketAdminService;

    public AdminSupportTicketsController(
        ISupportTicketAdminService supportTicketAdminService)
    {
        _supportTicketAdminService = supportTicketAdminService;
    }

    /// <summary>
    /// Gets all support tickets for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SupportTicketListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<SupportTicketListItemDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var tickets = await _supportTicketAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<SupportTicketListItemDto>>.Ok(
            tickets,
            "Support tickets retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single support ticket by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var ticket = await _supportTicketAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (ticket is null)
        {
            return NotFound(ApiResponse.Fail(
                "Support ticket was not found."
            ));
        }

        return Ok(ApiResponse<SupportTicketDetailDto>.Ok(
            ticket,
            "Support ticket retrieved successfully."
        ));
    }

    /// <summary>
    /// Adds an admin reply to a support ticket.
    /// </summary>
    [HttpPost("{id:guid}/messages")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> Reply(
        [FromRoute] Guid id,
        [FromBody] CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        if (adminUserId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated admin id was not found."
            ));
        }

        try
        {
            var ticket = await _supportTicketAdminService.ReplyAsync(
                adminUserId.Value,
                id,
                request,
                cancellationToken
            );

            if (ticket is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Support ticket was not found."
                ));
            }

            return Ok(ApiResponse<SupportTicketDetailDto>.Ok(
                ticket,
                "Admin reply added successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates support ticket status, priority, and assignee.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateSupportTicketStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _supportTicketAdminService.UpdateStatusAsync(
                id,
                request,
                cancellationToken
            );

            if (ticket is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Support ticket was not found."
                ));
            }

            return Ok(ApiResponse<SupportTicketDetailDto>.Ok(
                ticket,
                "Support ticket status updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Assigns or unassigns a support ticket.
    /// </summary>
    [HttpPatch("{id:guid}/assign")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> Assign(
        [FromRoute] Guid id,
        [FromBody] AssignSupportTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await _supportTicketAdminService.AssignAsync(
                id,
                request,
                cancellationToken
            );

            if (ticket is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Support ticket was not found."
                ));
            }

            return Ok(ApiResponse<SupportTicketDetailDto>.Ok(
                ticket,
                "Support ticket assignment updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Closes a support ticket by admin.
    /// </summary>
    [HttpPatch("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> Close(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var adminUserId = GetCurrentUserId();

        if (adminUserId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated admin id was not found."
            ));
        }

        var ticket = await _supportTicketAdminService.CloseAsync(
            adminUserId.Value,
            id,
            cancellationToken
        );

        if (ticket is null)
        {
            return NotFound(ApiResponse.Fail(
                "Support ticket was not found."
            ));
        }

        return Ok(ApiResponse<SupportTicketDetailDto>.Ok(
            ticket,
            "Support ticket closed successfully."
        ));
    }

    /// <summary>
    /// Soft deletes a support ticket and its messages.
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
        var deleted = await _supportTicketAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Support ticket was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Support ticket deleted successfully."
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