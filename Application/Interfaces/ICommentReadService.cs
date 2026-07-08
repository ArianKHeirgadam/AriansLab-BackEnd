using Application.DTOs.Comments;

namespace Application.Interfaces;

public interface ICommentReadService
{
    Task<List<CommentDto>> GetApprovedByBlogPostIdAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default);

    Task<CommentDto?> GetApprovedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}