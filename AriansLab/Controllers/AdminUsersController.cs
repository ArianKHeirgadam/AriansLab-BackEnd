using Application.Common.Models;
using Application.DTOs.Users;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminUsersController : ControllerBase
{
    private readonly IUserAdminService _userAdminService;

    public AdminUsersController(IUserAdminService userAdminService)
    {
        _userAdminService = userAdminService;
    }

    /// <summary>
    /// Gets all users for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<AdminUserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<AdminUserDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await _userAdminService.GetAllAsync(cancellationToken);

        return Ok(ApiResponse<List<AdminUserDto>>.Ok(
            users,
            "Users retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single user by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _userAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (user is null)
        {
            return NotFound(ApiResponse.Fail(
                "User was not found."
            ));
        }

        return Ok(ApiResponse<AdminUserDto>.Ok(
            user,
            "User retrieved successfully."
        ));
    }

    /// <summary>
    /// Creates a new user from admin panel.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> Create(
        [FromBody] CreateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                ApiResponse<AdminUserDto>.Ok(
                    user,
                    "User created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing user from admin panel.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userAdminService.UpdateAsync(
                id,
                request,
                cancellationToken
            );

            if (user is null)
            {
                return NotFound(ApiResponse.Fail(
                    "User was not found."
                ));
            }

            return Ok(ApiResponse<AdminUserDto>.Ok(
                user,
                "User updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Resets a user's password from admin panel.
    /// </summary>
    [HttpPatch("{id:guid}/reset-password")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> ResetPassword(
        [FromRoute] Guid id,
        [FromBody] ResetUserPasswordRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userAdminService.ResetPasswordAsync(
                id,
                request,
                cancellationToken
            );

            if (user is null)
            {
                return NotFound(ApiResponse.Fail(
                    "User was not found."
                ));
            }

            return Ok(ApiResponse<AdminUserDto>.Ok(
                user,
                "User password reset successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Activates a user account.
    /// </summary>
    [HttpPatch("{id:guid}/activate")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> Activate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _userAdminService.ActivateAsync(
            id,
            cancellationToken
        );

        if (user is null)
        {
            return NotFound(ApiResponse.Fail(
                "User was not found."
            ));
        }

        return Ok(ApiResponse<AdminUserDto>.Ok(
            user,
            "User activated successfully."
        ));
    }

    /// <summary>
    /// Deactivates a user account instead of physically deleting it.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AdminUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<AdminUserDto>>> Deactivate(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _userAdminService.DeactivateAsync(
            id,
            cancellationToken
        );

        if (user is null)
        {
            return NotFound(ApiResponse.Fail(
                "User was not found."
            ));
        }

        return Ok(ApiResponse<AdminUserDto>.Ok(
            user,
            "User deactivated successfully."
        ));
    }
}