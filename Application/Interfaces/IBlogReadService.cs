using Application.DTOs.Blog;
using Application.DTOs.Common;

namespace Application.Interfaces;

public interface IBlogReadService
{
    Task<PagedResultDto<BlogPostListItemDto>> GetPublishedPostsAsync(
        BlogPostQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<BlogPostDetailDto?> GetPublishedPostBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BlogCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}
