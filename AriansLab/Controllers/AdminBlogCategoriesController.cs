using Application.Common.Models;
using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AriansLab.Api.Controllers;

[ApiController]
[Route("api/admin/blog/categories")]
[Authorize(Roles = "Admin")]
public class AdminBlogCategoriesController : ControllerBase
{
    private readonly IBlogAdminCategoryService _blogAdminCategoryService;

    public AdminBlogCategoriesController(IBlogAdminCategoryService blogAdminCategoryService)
    {
        _blogAdminCategoryService = blogAdminCategoryService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> Create(
        [FromBody] CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _blogAdminCategoryService.CreateAsync(request, cancellationToken);

        return Ok(ApiResponse<BlogCategoryDto>.Ok(
            result,
            "Blog category created successfully."));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<BlogCategoryDto>>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _blogAdminCategoryService.UpdateAsync(id, request, cancellationToken);

        return Ok(ApiResponse<BlogCategoryDto>.Ok(
            result,
            "Blog category updated successfully."));
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _blogAdminCategoryService.DeleteAsync(id, cancellationToken);

        return Ok(ApiResponse<object>.Ok(
            null,
            "Blog category deleted successfully."));
    }
}