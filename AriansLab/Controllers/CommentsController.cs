using Application.Common.Models;
using Application.DTOs.Comments;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/comments")]
[AllowAnonymous]
[Produces("application/json")]
public class CommentsController : ControllerBase
{
    private readonly ICommentReadService _commentReadService;
    private readonly ICommentSubmitService _commentSubmitService;

    public CommentsController(
        ICommentReadService commentReadService,
        ICommentSubmitService commentSubmitService)
    {
        _commentReadService = commentReadService;
        _commentSubmitService = commentSubmitService;
    }

    /// <summary>
    /// Gets approved comments for a blog post.
    /// </summary>
    [HttpGet("blog-post/{blogPostId:guid}/approved")]
    [ProducesResponseType(typeof(ApiResponse<List<PublicCommentDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<PublicCommentDto>>>> GetApprovedByBlogPostId(
        [FromRoute] Guid blogPostId,
        CancellationToken cancellationToken)
    {
        var comments = await _commentReadService.GetApprovedByBlogPostIdAsync(
            blogPostId,
            cancellationToken
        );

        return Ok(ApiResponse<List<PublicCommentDto>>.Ok(
            comments,
            "Approved comments retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single approved comment by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<PublicCommentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<PublicCommentDto>>> GetApprovedById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var comment = await _commentReadService.GetApprovedByIdAsync(
            id,
            cancellationToken
        );

        if (comment is null)
        {
            return NotFound(ApiResponse.Fail(
                "Comment was not found."
            ));
        }

        return Ok(ApiResponse<PublicCommentDto>.Ok(
            comment,
            "Comment retrieved successfully."
        ));
    }

    /// <summary>
    /// Submits a new public comment. If the request has a valid bearer token, user id will be attached.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("public-write")]
    [ProducesResponseType(typeof(ApiResponse<PublicCommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PublicCommentDto>>> Create(
        [FromBody] CreateCommentRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetCurrentUserId();

            var comment = await _commentSubmitService.CreateAsync(
                request,
                userId,
                cancellationToken
            );

            return StatusCode(
                StatusCodes.Status201Created,
                ApiResponse<PublicCommentDto>.Ok(
                    ToPublicComment(comment),
                    "Comment submitted successfully and is waiting for approval."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    private static PublicCommentDto ToPublicComment(CommentDto comment)
    {
        return new PublicCommentDto
        {
            Id = comment.Id,
            BlogPostId = comment.BlogPostId,
            ParentCommentId = comment.ParentCommentId,
            FullName = comment.FullName,
            Message = comment.Message,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };
    }

    private Guid? GetCurrentUserId()
    {
        var userIdValue =
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? User.FindFirstValue("userId");

        if (Guid.TryParse(userIdValue, out var userId))
        {
            return userId;
        }

        return null;
    }
}
