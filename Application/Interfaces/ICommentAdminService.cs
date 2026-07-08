using Application.DTOs.Comments;

namespace Application.Interfaces;

public interface ICommentAdminService
{
    Task<List<CommentDto>> GetAllAsync(
        Guid? blogPostId = null,
        bool? isApproved = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<CommentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<CommentDto?> UpdateApprovalAsync(
        Guid id,
        UpdateCommentApprovalRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}