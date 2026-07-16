using Application.Common.Models;
using Application.DTOs.Profile;
using Application.Interfaces;
using AriansLab.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
[Produces("application/json")]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly AuthCookieSettings _cookieSettings;

    public ProfileController(
        IProfileService profileService,
        IOptions<AuthCookieSettings> cookieSettings)
    {
        _profileService = profileService;
        _cookieSettings = cookieSettings.Value;
    }

    /// <summary>
    /// Gets the authenticated user's profile from database.
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> GetMe(
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        var profile = await _profileService.GetMeAsync(
            userId.Value,
            cancellationToken
        );

        if (profile is null)
        {
            return NotFound(ApiResponse.Fail(
                "User profile was not found."
            ));
        }

        return Ok(ApiResponse<ProfileDto>.Ok(
            profile,
            "Profile retrieved successfully."
        ));
    }

    /// <summary>
    /// Updates the authenticated user's profile.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType(typeof(ApiResponse<ProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProfileDto>>> UpdateMe(
        [FromBody] UpdateProfileRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        try
        {
            var profile = await _profileService.UpdateMeAsync(
                userId.Value,
                request,
                cancellationToken
            );

            if (profile is null)
            {
                return NotFound(ApiResponse.Fail(
                    "User profile was not found."
                ));
            }

            return Ok(ApiResponse<ProfileDto>.Ok(
                profile,
                "Profile updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Changes the authenticated user's password after verifying current password.
    /// </summary>
    [HttpPatch("change-password")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> ChangePassword(
        [FromBody] ChangePasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized(ApiResponse.Fail(
                "Authenticated user id was not found."
            ));
        }

        try
        {
            var changed = await _profileService.ChangePasswordAsync(
                userId.Value,
                request,
                cancellationToken
            );

            if (!changed)
            {
                return NotFound(ApiResponse.Fail(
                    "User profile was not found."
                ));
            }

            DeleteAuthCookies();

            return Ok(ApiResponse.Ok(
                "Password changed successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    private void DeleteAuthCookies()
    {
        var sameSite = _cookieSettings.SameSite.Trim().ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Lax
        };
        var common = new CookieOptions
        {
            Secure = _cookieSettings.Secure,
            HttpOnly = true,
            SameSite = sameSite,
            Path = "/"
        };

        Response.Cookies.Delete(AuthCookieSettings.AccessCookieName, common);
        Response.Cookies.Delete(AuthCookieSettings.RememberCookieName, common);
        Response.Cookies.Delete(AuthCookieSettings.AntiforgeryCookieName, common);
        Response.Cookies.Delete(
            AuthCookieSettings.RefreshCookieName,
            new CookieOptions
            {
                Secure = _cookieSettings.Secure,
                HttpOnly = true,
                SameSite = sameSite,
                Path = "/api/Auth"
            });
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdValue))
        {
            return null;
        }

        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return null;
        }

        return userId;
    }
}
