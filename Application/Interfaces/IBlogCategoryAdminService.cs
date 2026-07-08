using Application.DTOs.Blog;
using Application.DTOs.Blog.Admin   ;

namespace Application.Interfaces;

public interface IBlogCategoryAdminService
{
    Task<List<BlogCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto> CreateAsync(
        CreateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto?> UpdateAsync(
        Guid id,
        UpdateBlogCategoryRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}