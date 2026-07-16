using Application.Common.Models;
using Application.DTOs.Blog.Admin;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/blog/posts")]
[Authorize(Roles = "Admin")]
public class AdminBlogPostsController : ControllerBase
{
    private readonly IBlogAdminPostService _blogAdminPostService;

    public AdminBlogPostsController(IBlogAdminPostService blogAdminPostService)
    {
        _blogAdminPostService = blogAdminPostService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResultDto<AdminBlogPostListItemDto>>>> GetPosts(
        [FromQuery] AdminBlogPostQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _blogAdminPostService.GetPostsAsync(
            parameters,
            cancellationToken);

        return Ok(ApiResponse<PagedResultDto<AdminBlogPostListItemDto>>.Ok(
            result,
            "Admin blog posts retrieved successfully."));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminBlogPostDetailDto>>> GetPostById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _blogAdminPostService.GetPostByIdAsync(
            id,
            cancellationToken);

        return Ok(ApiResponse<AdminBlogPostDetailDto>.Ok(
            result,
            "Admin blog post retrieved successfully."));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AdminBlogPostDetailDto>>> Create(
        [FromBody] CreateBlogPostRequestDto request,
        CancellationToken cancellationToken)
    {
        var authorId = GetCurrentUserId();

        var result = await _blogAdminPostService.CreateAsync(
            authorId,
            request,
            cancellationToken);

        return Ok(ApiResponse<AdminBlogPostDetailDto>.Ok(
            result,
            "Blog post created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AdminBlogPostDetailDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateBlogPostRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _blogAdminPostService.UpdateAsync(
            id,
            request,
            cancellationToken);

        return Ok(ApiResponse<AdminBlogPostDetailDto>.Ok(
            result,
            "Blog post updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object?>>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _blogAdminPostService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse<object?>.Ok(
            null,
            "Blog post deleted successfully."));
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId) ||
            !Guid.TryParse(userId, out var parsedUserId))
        {
            throw new UnauthorizedAccessException("User id claim is missing or invalid.");
        }

        return parsedUserId;
    }
}
