using Application.Common.Models;
using Application.DTOs.Auth;
using Application.DTOs.Logs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IActivityLogWriteService _activityLogWriteService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IActivityLogWriteService activityLogWriteService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _activityLogWriteService = activityLogWriteService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register(
        [FromBody] RegisterRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(
            request,
            cancellationToken
        );

        await WriteActivityLogSafelyAsync(
            result.User.Id,
            "Register",
            $"User '{result.User.UserName}' registered successfully.",
            cancellationToken
        );

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Registration completed successfully."
        ));
    }

    /// <summary>
    /// Logs in a user and returns access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login(
        [FromBody] LoginRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(
            request,
            cancellationToken
        );

        await WriteActivityLogSafelyAsync(
            result.User.Id,
            "Login",
            $"User '{result.User.UserName}' logged in successfully.",
            cancellationToken
        );

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Login completed successfully."
        ));
    }

    /// <summary>
    /// Refreshes access token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken(
        [FromBody] RefreshTokenRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(
            request,
            cancellationToken
        );

        return Ok(ApiResponse<AuthResponseDto>.Ok(
            result,
            "Token refreshed successfully."
        ));
    }

    /// <summary>
    /// Gets authenticated user information.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public ActionResult<ApiResponse<object>> Me()
    {
        var user = new
        {
            Id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = User.FindFirstValue(ClaimTypes.Email),
            UserName = User.FindFirstValue(ClaimTypes.Name),
            Role = User.FindFirstValue(ClaimTypes.Role),
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        };

        return Ok(ApiResponse<object>.Ok(
            user,
            "Authenticated user retrieved successfully."
        ));
    }

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
                    IpAddress = GetClientIpAddress(),
                    UserAgent = GetUserAgent()
                },
                cancellationToken
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to write activity log for user {UserId}.",
                userId
            );
        }
    }

    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"]
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    private string? GetUserAgent()
    {
        return Request.Headers.UserAgent.FirstOrDefault();
    }
}