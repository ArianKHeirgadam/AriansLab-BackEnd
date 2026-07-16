using Application.Common.Models;
using Application.DTOs.SocialMedias;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/social-media")]
[AllowAnonymous]
[Produces("application/json")]
public class SocialMediaController : ControllerBase
{
    private readonly ISocialMediaReadService _socialMediaReadService;

    public SocialMediaController(ISocialMediaReadService socialMediaReadService)
    {
        _socialMediaReadService = socialMediaReadService;
    }

    /// <summary>
    /// Gets active social media links for public website.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<SocialMediaDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<SocialMediaDto>>>> GetActive(
        CancellationToken cancellationToken)
    {
        var socialMediaLinks = await _socialMediaReadService.GetActiveAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<SocialMediaDto>>.Ok(
            socialMediaLinks,
            "Active social media links retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single active social media link by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SocialMediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialMediaDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var socialMedia = await _socialMediaReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (socialMedia is null)
        {
            return NotFound(ApiResponse.Fail(
                "Social media link was not found."
            ));
        }

        return Ok(ApiResponse<SocialMediaDto>.Ok(
            socialMedia,
            "Social media link retrieved successfully."
        ));
    }
}
