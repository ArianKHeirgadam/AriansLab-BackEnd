using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comments;

public class CreateCommentRequestDto
{
    public Guid BlogPostId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(3000)]
    public string Message { get; set; } = string.Empty;
}
