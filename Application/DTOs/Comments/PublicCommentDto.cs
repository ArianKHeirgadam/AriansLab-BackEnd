namespace Application.DTOs.Comments;

public sealed class PublicCommentDto
{
    public Guid Id { get; set; }

    public Guid BlogPostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
