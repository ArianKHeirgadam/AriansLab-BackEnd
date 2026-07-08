using Application.Common.Models;
using Application.DTOs.HeroSections;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/hero-sections")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminHeroSectionsController : ControllerBase
{
    private readonly IHeroSectionAdminService _heroSectionAdminService;

    public AdminHeroSectionsController(
        IHeroSectionAdminService heroSectionAdminService)
    {
        _heroSectionAdminService = heroSectionAdminService;
    }

    /// <summary>
    /// Gets all hero sections for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<HeroSectionDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<HeroSectionDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var heroSections = await _heroSectionAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<HeroSectionDto>>.Ok(
            heroSections,
            "Hero sections retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single hero section by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HeroSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeroSectionDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var heroSection = await _heroSectionAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (heroSection is null)
        {
            return NotFound(ApiResponse.Fail(
                "Hero section was not found."
            ));
        }

        return Ok(ApiResponse<HeroSectionDto>.Ok(
            heroSection,
            "Hero section retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new hero section.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<HeroSectionDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<HeroSectionDto>>> Create(
        [FromBody] CreateHeroSectionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var heroSection = await _heroSectionAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = heroSection.Id },
                ApiResponse<HeroSectionDto>.Ok(
                    heroSection,
                    "Hero section created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing hero section.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HeroSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeroSectionDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateHeroSectionRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var heroSection = await _heroSectionAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (heroSection is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Hero section was not found."
                ));
            }

            return Ok(ApiResponse<HeroSectionDto>.Ok(
                heroSection,
                "Hero section updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates hero section active status.
    /// </summary>
    [HttpPatch("{id:guid}/active-status")]
    [ProducesResponseType(typeof(ApiResponse<HeroSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeroSectionDto>>> UpdateActiveStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateHeroSectionActiveStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var heroSection = await _heroSectionAdminService.UpdateActiveStatusAsync(
            id,
            request,
            cancellationToken
        );

        if (heroSection is null)
        {
            return NotFound(ApiResponse.Fail(
                "Hero section was not found."
            ));
        }

        return Ok(ApiResponse<HeroSectionDto>.Ok(
            heroSection,
            "Hero section active status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes a hero section.
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
        var deleted = await _heroSectionAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Hero section was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Hero section deleted successfully."
        ));
    }
}