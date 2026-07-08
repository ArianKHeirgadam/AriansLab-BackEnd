namespace Application.DTOs.Comments;

public class CreateCommentRequestDto
{
    public Guid BlogPostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}