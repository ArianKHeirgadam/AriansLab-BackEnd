using Application.Common.Models;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/site-settings")]
[Produces("application/json")]
public class SiteSettingsController : ControllerBase
{
    private readonly ISiteSettingReadService _siteSettingReadService;

    public SiteSettingsController(ISiteSettingReadService siteSettingReadService)
    {
        _siteSettingReadService = siteSettingReadService;
    }

    /// <summary>
    /// Gets the latest public site setting.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SiteSettingDto>>> GetCurrent(
        CancellationToken cancellationToken)
    {
        var siteSetting = await _siteSettingReadService.GetCurrentAsync(
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
    /// Gets a single public site setting by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SiteSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SiteSettingDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var siteSetting = await _siteSettingReadService.GetByIdAsync(
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
}