using Application.Common.Models;
using Application.DTOs.Files;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/project-files")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminProjectFilesController : ControllerBase
{
    private readonly IProjectFileAdminService _projectFileAdminService;

    public AdminProjectFilesController(
        IProjectFileAdminService projectFileAdminService)
    {
        _projectFileAdminService = projectFileAdminService;
    }

    /// <summary>
    /// Gets all project files for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectFileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<ProjectFileDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var files = await _projectFileAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<ProjectFileDto>>.Ok(
            files,
            "Project files retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets all files for a specific project.
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<ProjectFileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<ProjectFileDto>>>> GetByProjectId(
        [FromRoute] Guid projectId,
        CancellationToken cancellationToken)
    {
        var files = await _projectFileAdminService.GetByProjectIdAsync(
            projectId,
            cancellationToken
        );

        return Ok(ApiResponse<List<ProjectFileDto>>.Ok(
            files,
            "Project files retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single project file by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectFileDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var file = await _projectFileAdminService.GetByIdAsync(
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

    /// <summary>
    /// Creates a project file metadata record.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProjectFileDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ProjectFileDto>>> Create(
        [FromBody] CreateProjectFileRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var file = await _projectFileAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = file.Id },
                ApiResponse<ProjectFileDto>.Ok(
                    file,
                    "Project file created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates a project file metadata record.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ProjectFileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProjectFileDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateProjectFileRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var file = await _projectFileAdminService.UpdateAsync(
                id,
                request,
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
                "Project file updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a project file metadata record.
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
        var deleted = await _projectFileAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Project file was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Project file deleted successfully."
        ));
    }
}