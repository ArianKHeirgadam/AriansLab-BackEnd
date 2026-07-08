using Application.Common.Models;
using Application.DTOs.Logs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/audit-logs")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminAuditLogsController : ControllerBase
{
    private readonly IAuditLogReadService _auditLogReadService;

    public AdminAuditLogsController(
        IAuditLogReadService auditLogReadService)
    {
        _auditLogReadService = auditLogReadService;
    }

    /// <summary>
    /// Gets audit logs for admin panel with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AuditLogDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AuditLogDto>>>> GetAll(
        [FromQuery] Guid? userId,
        [FromQuery] string? action,
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogReadService.GetAllAsync(
            userId,
            action,
            entityName,
            entityId,
            from,
            to,
            skip,
            take,
            cancellationToken
        );

        return Ok(ApiResponse<List<AuditLogDto>>.Ok(
            logs,
            "Audit logs retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single audit log by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AuditLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AuditLogDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var log = await _auditLogReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (log is null)
        {
            return NotFound(ApiResponse.Fail(
                "Audit log was not found."
            ));
        }

        return Ok(ApiResponse<AuditLogDto>.Ok(
            log,
            "Audit log retrieved successfully."
        ));
    }
}