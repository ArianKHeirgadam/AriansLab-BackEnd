using Application.Common.Models;
using Application.DTOs.Blog;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/blog-categories")]
[AllowAnonymous]
[Produces("application/json")]
public class BlogCategoriesController : ControllerBase
{
    private readonly IBlogCategoryReadService _blogCategoryReadService;

    public BlogCategoriesController(
        IBlogCategoryReadService blogCategoryReadService)
    {
        _blogCategoryReadService = blogCategoryReadService;
    }

    /// <summary>
    /// Gets all public blog categories.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BlogCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<BlogCategoryDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _blogCategoryReadService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<BlogCategoryDto>>.Ok(
            categories,
            "Blog categories retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single public blog category by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var category = await _blogCategoryReadService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (category is null)
        {
            return NotFound(ApiResponse.Fail(
                "Blog category was not found."
            ));
        }

        return Ok(ApiResponse<BlogCategoryDto>.Ok(
            category,
            "Blog category retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single public blog category by slug.
    /// </summary>
    [HttpGet("slug/{slug}")]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> GetBySlug(
        [FromRoute] string slug,
        CancellationToken cancellationToken)
    {
        var category = await _blogCategoryReadService.GetBySlugAsync(
            slug,
            cancellationToken
        );

        if (category is null)
        {
            return NotFound(ApiResponse.Fail(
                "Blog category was not found."
            ));
        }

        return Ok(ApiResponse<BlogCategoryDto>.Ok(
            category,
            "Blog category retrieved successfully."
        ));
    }
}
