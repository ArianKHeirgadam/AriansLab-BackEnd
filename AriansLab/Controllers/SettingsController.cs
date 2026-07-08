using Application.Common.Models;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/settings")]
[Produces("application/json")]
public class SettingsController : ControllerBase
{
    private readonly ISettingReadService _settingReadService;

    public SettingsController(ISettingReadService settingReadService)
    {
        _settingReadService = settingReadService;
    }

    /// <summary>
    /// Gets the latest public setting.
    /// </summary>
    [HttpGet("current")]
    [ProducesResponseType(typeof(ApiResponse<SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> GetCurrent(
        CancellationToken cancellationToken)
    {
        var setting = await _settingReadService.GetCurrentAsync(
            cancellationToken
        );

        if (setting is null)
        {
            return NotFound(ApiResponse.Fail(
                "Setting was not found."
            ));
        }

        return Ok(ApiResponse<SettingDto>.Ok(
            setting,
            "Setting retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single public setting by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var setting = await _settingReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (setting is null)
        {
            return NotFound(ApiResponse.Fail(
                "Setting was not found."
            ));
        }

        return Ok(ApiResponse<SettingDto>.Ok(
            setting,
            "Setting retrieved successfully."
        ));
    }
}