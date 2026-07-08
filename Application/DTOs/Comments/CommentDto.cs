namespace Application.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }

    public Guid BlogPostId { get; set; }

    public string? BlogPostTitle { get; set; }

    public Guid? UserId { get; set; }

    public string? UserFullName { get; set; }

    public string? UserEmail { get; set; }

    public Guid? ParentCommentId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}