using Application.Common.Models;
using Application.DTOs.Comments;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/comments")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminCommentsController : ControllerBase
{
    private readonly ICommentAdminService _commentAdminService;

    public AdminCommentsController(ICommentAdminService commentAdminService)
    {
        _commentAdminService = commentAdminService;
    }

    /// <summary>
    /// Gets comments for admin panel with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<CommentDto>>>> GetAll(
        [FromQuery] Guid? blogPostId,
        [FromQuery] bool? isApproved,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var comments = await _commentAdminService.GetAllAsync(
            blogPostId,
            isApproved,
            skip,
            take,
            cancellationToken
        );

        return Ok(ApiResponse<List<CommentDto>>.Ok(
            comments,
            "Comments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single comment by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CommentDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var comment = await _commentAdminService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (comment is null)
        {
            return NotFound(ApiResponse.Fail(
                "Comment was not found."
            ));
        }

        return Ok(ApiResponse<CommentDto>.Ok(
            comment,
            "Comment retrieved successfully."
        ));
    }

    /// <summary>
    /// Updates comment approval status.
    /// </summary>
    [HttpPatch("{id:guid}/approval")]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CommentDto>>> UpdateApproval(
        [FromRoute] Guid id,
        [FromBody] UpdateCommentApprovalRequestDto request,
        CancellationToken cancellationToken)
    {
        var comment = await _commentAdminService.UpdateApprovalAsync(
            id,
            request,
            cancellationToken
        );

        if (comment is null)
        {
            return NotFound(ApiResponse.Fail(
                "Comment was not found."
            ));
        }

        return Ok(ApiResponse<CommentDto>.Ok(
            comment,
            "Comment approval status updated successfully."
        ));
    }

    /// <summary>
    /// Soft deletes a comment.
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
        var deleted = await _commentAdminService.DeleteAsync(
            id,
            cancellationToken
        );

        if (!deleted)
        {
            return NotFound(ApiResponse.Fail(
                "Comment was not found."
            ));
        }

        return Ok(ApiResponse.Ok(
            "Comment deleted successfully."
        ));
    }
}