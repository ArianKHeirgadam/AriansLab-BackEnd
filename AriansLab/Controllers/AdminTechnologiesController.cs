using Application.Common.Models;
using Application.DTOs.Technologies;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/technologies")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminTechnologiesController : ControllerBase
{
    private readonly ITechnologyAdminService _technologyAdminService;

    public AdminTechnologiesController(
        ITechnologyAdminService technologyAdminService)
    {
        _technologyAdminService = technologyAdminService;
    }

    /// <summary>
    /// Gets all technologies for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<TechnologyDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<TechnologyDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var technologies = await _technologyAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<TechnologyDto>>.Ok(
            technologies,
            "Technologies retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single technology by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TechnologyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TechnologyDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var technology = await _technologyAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (technology is null)
        {
            return NotFound(ApiResponse.Fail(
                "Technology was not found."
            ));
        }

        return Ok(ApiResponse<TechnologyDto>.Ok(
            technology,
            "Technology retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new technology.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TechnologyDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<TechnologyDto>>> Create(
        [FromBody] CreateTechnologyRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var technology = await _technologyAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = technology.Id },
                ApiResponse<TechnologyDto>.Ok(
                    technology,
                    "Technology created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing technology.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TechnologyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<TechnologyDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateTechnologyRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var technology = await _technologyAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (technology is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Technology was not found."
                ));
            }

            return Ok(ApiResponse<TechnologyDto>.Ok(
                technology,
                "Technology updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a technology.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _technologyAdminService.DeleteAsync(
                id,
                cancellationToken
            );

            return Ok(ApiResponse.Ok(
                "Technology deleted successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}