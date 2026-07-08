using Application.Common.Models;
using Application.DTOs.SocialMedias;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/social-media")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSocialMediaController : ControllerBase
{
    private readonly ISocialMediaAdminService _socialMediaAdminService;

    public AdminSocialMediaController(
        ISocialMediaAdminService socialMediaAdminService)
    {
        _socialMediaAdminService = socialMediaAdminService;
    }

    /// <summary>
    /// Gets all social media links for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SocialMediaDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<SocialMediaDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var socialMediaLinks = await _socialMediaAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<SocialMediaDto>>.Ok(
            socialMediaLinks,
            "Social media links retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single social media link by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SocialMediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialMediaDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var socialMedia = await _socialMediaAdminService.GetByIdAsync(
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

    /// <summary>
    /// Creates a new social media link.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SocialMediaDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SocialMediaDto>>> Create(
        [FromBody] CreateSocialMediaRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var socialMedia = await _socialMediaAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = socialMedia.Id },
                ApiResponse<SocialMediaDto>.Ok(
                    socialMedia,
                    "Social media link created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing social media link.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SocialMediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialMediaDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSocialMediaRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var socialMedia = await _socialMediaAdminService.UpdateAsync(
                id,
                request,
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
                "Social media link updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates social media link active status.
    /// </summary>
    [HttpPatch("{id:guid}/active-status")]
    [ProducesResponseType(typeof(ApiResponse<SocialMediaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SocialMediaDto>>> UpdateActiveStatus(
        [FromRoute] Guid id,
        [FromBody] UpdateSocialMediaActiveStatusRequestDto request,
        CancellationToken cancellationToken)
    {
        var socialMedia = await _socialMediaAdminService.UpdateActiveStatusAsync(
            id,
            request,
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
            "Social media link active status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes a social media link.
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
        var deleted = await _socialMediaAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Social media link was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Social media link deleted successfully."
        ));
    }
}