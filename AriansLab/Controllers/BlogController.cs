using Application.Common.Models;
using Application.DTOs.Blog;
using Application.DTOs.Common;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/blog")]
[AllowAnonymous]
public class BlogController : ControllerBase
{
    private readonly IBlogReadService _blogReadService;

    public BlogController(IBlogReadService blogReadService)
    {
        _blogReadService = blogReadService;
    }

    [HttpGet("posts")]
    public async Task<ActionResult<ApiResponse<PagedResultDto<BlogPostListItemDto>>>> GetPosts(
        [FromQuery] BlogPostQueryParameters parameters,
        CancellationToken cancellationToken)
    {
        var result = await _blogReadService.GetPublishedPostsAsync(parameters, cancellationToken);

        return Ok(ApiResponse<PagedResultDto<BlogPostListItemDto>>.Ok(
            result,
            "Blog posts retrieved successfully."));
    }

    [HttpGet("posts/{slug}")]
    public async Task<ActionResult<ApiResponse<BlogPostDetailDto>>> GetPostBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var result = await _blogReadService.GetPublishedPostBySlugAsync(slug, cancellationToken);

        if (result is null)
        {
            return NotFound(ApiResponse<BlogPostDetailDto>.Fail(
                "Blog post was not found."));
        }

        return Ok(ApiResponse<BlogPostDetailDto>.Ok(
            result,
            "Blog post retrieved successfully."));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<BlogCategoryDto>>>> GetCategories(
        CancellationToken cancellationToken)
    {
        var result = await _blogReadService.GetCategoriesAsync(cancellationToken);

        return Ok(ApiResponse<IReadOnlyList<BlogCategoryDto>>.Ok(
            result,
            "Blog categories retrieved successfully."));
    }
}