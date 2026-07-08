using Application.Common.Models;
using Application.DTOs.Projects;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectReadService _projectReadService;

    public ProjectsController(IProjectReadService projectReadService)
    {
        _projectReadService = projectReadService;
    }

    /// <summary>
    /// Gets projects that belong to the authenticated user.
    /// </summary>
    [HttpGet("my")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectListItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<ProjectListItemDto>>>> GetMyProjects(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var projects = await _projectReadService.GetMyProjectsAsync(
            userId.Value,
            cancellationToken
        );

        return Ok(ApiResponse<List<ProjectListItemDto>>.Ok(
            projects,
            "Projects retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single project that belongs to the authenticated user.
    /// </summary>
    [HttpGet("my/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> GetMyProjectById(
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

        var project = await _projectReadService.GetMyProjectByIdAsync(
            userId.Value,
            id,
            cancellationToken
        );

        if (project is null)
        {
            return NotFound(ApiResponse.Fail(
                "Project was not found."
            ));
        }

        return Ok(ApiResponse<ProjectDetailDto>.Ok(
            project,
            "Project retrieved successfully."
        ));
    }

    /// <summary>
    /// Updates the authenticated user's comment on one of their projects.
    /// </summary>
    [HttpPatch("my/{id:guid}/customer-comment")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> UpdateMyCustomerComment(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectCustomerCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var project = await _projectReadService.UpdateMyCustomerCommentAsync(
            userId.Value,
            id,
            request,
            cancellationToken
        );

        if (project is null)
        {
            return NotFound(ApiResponse.Fail(
                "Project was not found."
            ));
        }

        return Ok(ApiResponse<ProjectDetailDto>.Ok(
            project,
            "Customer comment updated successfully."
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