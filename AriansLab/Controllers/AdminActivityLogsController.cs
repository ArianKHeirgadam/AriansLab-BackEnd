using Application.Common.Models;
using Application.DTOs.Logs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/activity-logs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminActivityLogsController : ControllerBase
{
    private readonly IActivityLogReadService _activityLogReadService;

    public AdminActivityLogsController(
        IActivityLogReadService activityLogReadService)
    {
        _activityLogReadService = activityLogReadService;
    }

    /// <summary>
    /// Gets activity logs for admin panel with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ActivityLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<ActivityLogDto>>>> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] string? activity,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _activityLogReadService.GetAllAsync(
            userId,
            activity,
            from,
            to,
            skip,
            take,
            cancellationToken
        );

        return Ok(ApiResponse<List<ActivityLogDto>>.Ok(
            logs,
            "Activity logs retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single activity log by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ActivityLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ActivityLogDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var log = await _activityLogReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (log is null)
        {
            return NotFound(ApiResponse.Fail(
                "Activity log was not found."
            ));
        }

        return Ok(ApiResponse<ActivityLogDto>.Ok(
            log,
            "Activity log retrieved successfully."
        ));
    }
}