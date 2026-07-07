using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin;

namespace Application.Interfaces;

public interface IBlogAdminCategoryService
{
    Task<BlogCategoryDto> CreateAsync(
        CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto> UpdateAsync(
        Guid id,
        UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}