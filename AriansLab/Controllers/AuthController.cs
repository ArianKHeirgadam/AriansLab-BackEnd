using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Logs;
using Application.Interfaces;
using AriansLab.Api.Security;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IActivityLogWriteService _activityLogWriteService;
    private readonly IAntiforgery _antiforgery;
    private readonly AuthCookieSettings _cookieSettings;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IActivityLogWriteService activityLogWriteService,
        IAntiforgery antiforgery,
        IOptions<AuthCookieSettings> cookieSettings,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _activityLogWriteService = activityLogWriteService;
        _antiforgery = antiforgery;
        _cookieSettings = cookieSettings.Value;
        _logger = logger;
    }

    [HttpGet("csrf-token")]
    [AllowAnonymous]
    [EnableRateLimiting("csrf")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public ActionResult<ApiResponse<CsrfTokenResponseDto>> GetCsrfToken()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(ApiResponse<CsrfTokenResponseDto>.Ok(
            new CsrfTokenResponseDto { Token = tokens.RequestToken ?? string.Empty },
            "CSRF token created successfully."));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        WriteAuthCookies(result, persistent: false);

        await WriteActivityLogSafelyAsync(
            result.User.Id,
            "Register",
            $"User '{result.User.UserName}' registered successfully.",
            cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Registration completed successfully."));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        WriteAuthCookies(result, request.RememberMe);

        await WriteActivityLogSafelyAsync(
            result.User.Id,
            "Login",
            $"User '{result.User.UserName}' logged in successfully.",
            cancellationToken);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Login completed successfully."));
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        CancellationToken cancellationToken)
    {
        if (!Request.Cookies.TryGetValue(AuthCookieSettings.RefreshCookieName, out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(ApiResponse.Fail("Refresh token is invalid or expired."));
        }

        var result = await _authService.RefreshTokenAsync(
            new RefreshTokenRequestDto { RefreshToken = refreshToken },
            cancellationToken);
        var persistent = Request.Cookies.ContainsKey(AuthCookieSettings.RememberCookieName);
        WriteAuthCookies(result, persistent);

        return Ok(ApiResponse<AuthResponseDto>.Ok(result, "Token refreshed successfully."));
    }

    [HttpPost("logout")]
    [AllowAnonymous]
    [EnableRateLimiting("auth")]
    public async Task<ActionResult<ApiResponse>> Logout(CancellationToken cancellationToken)
    {
        if (Request.Cookies.TryGetValue(AuthCookieSettings.RefreshCookieName, out var refreshToken))
        {
            await _authService.RevokeRefreshTokenAsync(refreshToken, cancellationToken);
        }

        DeleteAuthCookies();
        return Ok(ApiResponse.Ok("Logout completed successfully."));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UserDto>>> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("Authenticated user id was not found."));
        }

        var user = await _authService.GetUserAsync(userId, cancellationToken);
        if (user is null)
        {
            DeleteAuthCookies();
            return Unauthorized(ApiResponse.Fail("Authenticated user is no longer active."));
        }

        return Ok(ApiResponse<UserDto>.Ok(user, "Authenticated user retrieved successfully."));
    }

    private void WriteAuthCookies(AuthResponseDto response, bool persistent)
    {
        var sameSite = ParseSameSite(_cookieSettings.SameSite);
        Response.Cookies.Append(
            AuthCookieSettings.AccessCookieName,
            response.AccessToken,
            CreateCookieOptions(sameSite, httpOnly: true, response.AccessTokenExpiresAt));

        var refreshOptions = CreateCookieOptions(
            sameSite,
            httpOnly: true,
            persistent ? response.RefreshTokenExpiresAt : null,
            "/api/Auth");
        Response.Cookies.Append(AuthCookieSettings.RefreshCookieName, response.RefreshToken, refreshOptions);

        var rememberOptions = CreateCookieOptions(
            sameSite,
            httpOnly: true,
            persistent ? response.RefreshTokenExpiresAt : null);
        Response.Cookies.Append(AuthCookieSettings.RememberCookieName, "1", rememberOptions);
    }

    private void DeleteAuthCookies()
    {
        var sameSite = ParseSameSite(_cookieSettings.SameSite);
        Response.Cookies.Delete(
            AuthCookieSettings.AccessCookieName,
            CreateCookieOptions(sameSite, httpOnly: true, expires: null));
        Response.Cookies.Delete(
            AuthCookieSettings.RefreshCookieName,
            CreateCookieOptions(sameSite, httpOnly: true, expires: null, path: "/api/Auth"));
        Response.Cookies.Delete(
            AuthCookieSettings.RememberCookieName,
            CreateCookieOptions(sameSite, httpOnly: true, expires: null));
        Response.Cookies.Delete(
            AuthCookieSettings.AntiforgeryCookieName,
            CreateCookieOptions(sameSite, httpOnly: true, expires: null));
    }

    private CookieOptions CreateCookieOptions(
        SameSiteMode sameSite,
        bool httpOnly,
        DateTime? expires,
        string path = "/")
    {
        return new CookieOptions
        {
            HttpOnly = httpOnly,
            Secure = _cookieSettings.Secure,
            SameSite = sameSite,
            Path = path,
            Expires = expires is null ? null : new DateTimeOffset(DateTime.SpecifyKind(expires.Value, DateTimeKind.Utc)),
            IsEssential = true
        };
    }

    private static SameSiteMode ParseSameSite(string value) => value.Trim().ToLowerInvariant() switch
    {
        "strict" => SameSiteMode.Strict,
        "none" => SameSiteMode.None,
        _ => SameSiteMode.Lax
    };

    private async Task WriteActivityLogSafelyAsync(
        Guid userId,
        string activity,
        string description,
        CancellationToken cancellationToken)
    {
        try
        {
            await _activityLogWriteService.CreateAsync(
                new CreateActivityLogRequestDto
                {
                    UserId = userId,
                    Activity = activity,
                    Description = description,
                    IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                    UserAgent = Request.Headers.UserAgent.FirstOrDefault()
                },
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Failed to write activity log for user {UserId}.", userId);
        }
    }
}
