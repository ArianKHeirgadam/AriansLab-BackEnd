using Application.Common.Models;
using Application.DTOs.SupportTickets;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/support-tickets")]
[Authorize]
[Produces("application/json")]
public class SupportTicketsController : ControllerBase
{
    private readonly ISupportTicketService _supportTicketService;

    public SupportTicketsController(ISupportTicketService supportTicketService)
    {
        _supportTicketService = supportTicketService;
    }

    /// <summary>
    /// Gets support tickets that belong to the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<SupportTicketListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<SupportTicketListItemDto>>>> GetMyTickets(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var tickets = await _supportTicketService.GetMyTicketsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<SupportTicketListItemDto>>.Ok(
            tickets,
            "Support tickets retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single support ticket that belongs to the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> GetMyTicketById(
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

        var ticket = await _supportTicketService.GetMyTicketByIdAsync(
            userId.Value,
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
    /// Creates a new support ticket for the authenticated user.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("public-write")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> Create(
        [FromBody] CreateSupportTicketRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        try
        {
            var ticket = await _supportTicketService.CreateAsync(
                userId.Value,
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetMyTicketById),
                new { id = ticket.Id },
                ApiResponse<SupportTicketDetailDto>.Ok(
                    ticket,
                    "Support ticket created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Adds a new message to one of the authenticated user's support tickets.
    /// </summary>
    [HttpPost("my/{id:guid}/messages")]
    [EnableRateLimiting("public-write")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> AddMessage(
        [FromRoute] Guid id,
        [FromBody] CreateTicketMessageRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        try
        {
            var ticket = await _supportTicketService.AddMessageAsync(
                userId.Value,
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
                "Ticket message added successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Closes one of the authenticated user's support tickets.
    /// </summary>
    [HttpPatch("my/{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<SupportTicketDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SupportTicketDetailDto>>> CloseMyTicket(
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

        var ticket = await _supportTicketService.CloseMyTicketAsync(
            userId.Value,
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
