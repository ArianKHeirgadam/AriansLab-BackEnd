using Application.Common.Models;
using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/project-files")]
[Authorize]
[Produces("application/json")]
public class ProjectFilesController : ControllerBase
{
    private readonly IProjectFileService _projectFileService;

    public ProjectFilesController(IProjectFileService projectFileService)
    {
        _projectFileService = projectFileService;
    }

    /// <summary>
    /// Gets files for one of the authenticated user's projects.
    /// </summary>
    [HttpGet("my/project/{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectFileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<ProjectFileDto>>>> GetMyProjectFiles(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var files = await _projectFileService.GetMyProjectFilesAsync(
            userId.Value,
            projectId,
            cancellationToken
        );

        return Ok(ApiResponse<List<ProjectFileDto>>.Ok(
            files,
            "Project files retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single project file that belongs to the authenticated user's project.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectFileDto>>> GetMyProjectFileById(
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

        var file = await _projectFileService.GetMyProjectFileByIdAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (file is null)
        {
            return NotFound(ApiResponse.Fail(
                "Project file was not found."
            ));
        }

        return Ok(ApiResponse<ProjectFileDto>.Ok(
            file,
            "Project file retrieved successfully."
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