using Application.DTOs.Comments;

namespace Application.Interfaces;

public interface ICommentSubmitService
{
    Task<CommentDto> CreateAsync(
        CreateCommentRequestDto request,
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}