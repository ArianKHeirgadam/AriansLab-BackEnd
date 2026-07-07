using Application.DTOs.Blog.Admin;
using Application.DTOs.Common;

namespace Application.Interfaces;

public interface IBlogAdminPostService
{
    Task<PagedResultDto<AdminBlogPostListItemDto>> GetPostsAsync(
        AdminBlogPostQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<AdminBlogPostDetailDto> GetPostByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminBlogPostDetailDto> CreateAsync(
        Guid authorId,
        CreateBlogPostRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminBlogPostDetailDto> UpdateAsync(
        Guid id,
        UpdateBlogPostRequestDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}