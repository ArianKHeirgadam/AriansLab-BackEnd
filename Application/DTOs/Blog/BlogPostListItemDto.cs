namespace Application.DTOs.Blog;

public class BlogPostListItemDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Excerpt { get; set; } = string.Empty;

    public string CoverImage { get; set; } = string.Empty;

    public int ReadTime { get; set; }

    public int ViewCount { get; set; }

    public DateTime? PublishedAt { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategorySlug { get; set; } = string.Empty;

    public string AuthorName { get; set; } = string.Empty;

    public string? Keywords { get; set; }
}
