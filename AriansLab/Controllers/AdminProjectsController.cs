using Application.Common.Models;
using Application.DTOs.Projects;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/projects")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminProjectsController : ControllerBase
{
    private readonly IProjectAdminService _projectAdminService;

    public AdminProjectsController(IProjectAdminService projectAdminService)
    {
        _projectAdminService = projectAdminService;
    }

    /// <summary>
    /// Gets all projects for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectDetailDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<ProjectDetailDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var projects = await _projectAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<ProjectDetailDto>>.Ok(
            projects,
            "Projects retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single project by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var project = await _projectAdminService.GetByIdAsync(
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
    /// Creates a new project for a customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> Create(
        [FromBody] CreateProjectRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = project.Id },
                ApiResponse<ProjectDetailDto>.Ok(
                    project,
                    "Project created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing project.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectAdminService.UpdateAsync(
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
                "Project updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates project status and progress.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<ProjectDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectDetailDto>>> UpdateStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var project = await _projectAdminService.UpdateStatusAsync(
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
                "Project status updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes an existing project.
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
        var deleted = await _projectAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Project was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Project deleted successfully."
        ));
    }
}