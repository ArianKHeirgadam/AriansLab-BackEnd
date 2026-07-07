namespace Application.DTOs.Blog.Admin;

public class UpdateBlogPostRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string CoverImage { get; set; } = string.Empty;

    public int ReadTime { get; set; } = 1;

    public bool IsPublished { get; set; }

    public DateTime? PublishedAt { get; set; }

    public string? SeoTitle { get; set; }

    public string? SeoDescription { get; set; }

    public string? Keywords { get; set; }

    public Guid CategoryId { get; set; }
}