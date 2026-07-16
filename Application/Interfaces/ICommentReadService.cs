using Application.DTOs.Comments;

namespace Application.Interfaces;

public interface ICommentReadService
{
    Task<List<PublicCommentDto>> GetApprovedByBlogPostIdAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default);

    Task<PublicCommentDto?> GetApprovedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
