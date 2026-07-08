using Application.Common.Models;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/site-settings")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSiteSettingsController : ControllerBase
{
    private readonly ISiteSettingAdminService _siteSettingAdminService;

    public AdminSiteSettingsController(
        ISiteSettingAdminService siteSettingAdminService)
    {
        _siteSettingAdminService = siteSettingAdminService;
    }

    /// <summary>
    /// Gets all site settings for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SiteSettingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<SiteSettingDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var siteSettings = await _siteSettingAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<SiteSettingDto>>.Ok(
            siteSettings,
            "Site settings retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single site setting by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SiteSettingDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var siteSetting = await _siteSettingAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (siteSetting is null)
        {
            return NotFound(ApiResponse.Fail(
                "Site setting was not found."
            ));
        }

        return Ok(ApiResponse<SiteSettingDto>.Ok(
            siteSetting,
            "Site setting retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new site setting.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SiteSettingDto>>> Create(
        [FromBody] CreateSiteSettingRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var siteSetting = await _siteSettingAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = siteSetting.Id },
                ApiResponse<SiteSettingDto>.Ok(
                    siteSetting,
                    "Site setting created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing site setting.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SiteSettingDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSiteSettingRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var siteSetting = await _siteSettingAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (siteSetting is null)
            {
                return NotFound(ApiResponse.Fail(
                    "Site setting was not found."
                ));
            }

            return Ok(ApiResponse<SiteSettingDto>.Ok(
                siteSetting,
                "Site setting updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a site setting.
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
        var deleted = await _siteSettingAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Site setting was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Site setting deleted successfully."
        ));
    }
}