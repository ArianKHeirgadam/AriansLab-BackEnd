using Application.Common.Models;
using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/blog-categories")]
[Authorize(Roles = "Admin")]
[Produces("application/json")]
public class AdminBlogCategoriesController : ControllerBase
{
    private readonly IBlogCategoryAdminService _blogCategoryAdminService;

    public AdminBlogCategoriesController(
        IBlogCategoryAdminService blogCategoryAdminService)
    {
        _blogCategoryAdminService = blogCategoryAdminService;
    }

    /// <summary>
    /// Gets all blog categories for admin panel.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<BlogCategoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<BlogCategoryDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var categories = await _blogCategoryAdminService.GetAllAsync(
            cancellationToken
        );

        return Ok(ApiResponse<List<BlogCategoryDto>>.Ok(
            categories,
            "Blog categories retrieved successfully."
        ));
    }

    /// <summary>
    /// Gets a single blog category by id for admin panel.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var category = await _blogCategoryAdminService.GetByIdAsync(
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
    /// Creates a new blog category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> Create(
        [FromBody] CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _blogCategoryAdminService.CreateAsync(
                request,
                cancellationToken
            );

            return CreatedAtAction(
                nameof(GetById),
                new { id = category.Id },
                ApiResponse<BlogCategoryDto>.Ok(
                    category,
                    "Blog category created successfully."
                )
            );
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Updates an existing blog category.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var category = await _blogCategoryAdminService.UpdateAsync(
                id,
                request,
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
                "Blog category updated successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>
    /// Soft deletes a blog category.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _blogCategoryAdminService.DeleteAsync(
                id,
                cancellationToken
            );

            if (!deleted)
            {
                return NotFound(ApiResponse.Fail(
                    "Blog category was not found."
                ));
            }

            return Ok(ApiResponse.Ok(
                "Blog category deleted successfully."
            ));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }
}