using Application.Common.Models;
using Application.DTOs.HeroSections;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/hero-sections")]
[AllowAnonymous]
[Produces("application/json")]
public class HeroSectionsController : ControllerBase
{
    private readonly IHeroSectionReadService _heroSectionReadService;

    public HeroSectionsController(IHeroSectionReadService heroSectionReadService)
    {
        _heroSectionReadService = heroSectionReadService;
    }

    /// <summary>
    /// Gets active hero sections for public home page.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<HeroSectionDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<HeroSectionDto>>>> GetActive(
        CancellationToken cancellationToken)
    {
        var heroSections = await _heroSectionReadService.GetActiveAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<HeroSectionDto>>.Ok(
            heroSections,
            "Active hero sections retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single active hero section by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<HeroSectionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<HeroSectionDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var heroSection = await _heroSectionReadService.GetByIdAsync(
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
}
