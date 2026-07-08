using Application.Common.Models;
using Application.DTOs.Settings;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/settings")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminSettingsController : ControllerBase
{
    private readonly ISettingAdminService _settingAdminService;

    public AdminSettingsController(ISettingAdminService settingAdminService)
    {
        _settingAdminService = settingAdminService;
    }

    /// <summary>
    /// Gets all settings for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<SettingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<SettingDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var settings = await _settingAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<SettingDto>>.Ok(
            settings,
            "Settings retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single setting by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var setting = await _settingAdminService.GetByIdAsync(
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

    /// <summary>
    /// Creates a new setting.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<SettingDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> Create(
        [FromBody] CreateSettingRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _settingAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = setting.Id },
                ApiResponse<SettingDto>.Ok(
                    setting,
                    "Setting created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing setting.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<SettingDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateSettingRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var setting = await _settingAdminService.UpdateAsync(
                id,
                request,
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
                "Setting updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a setting.
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
        var deleted = await _settingAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Setting was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Setting deleted successfully."
        ));
    }
}