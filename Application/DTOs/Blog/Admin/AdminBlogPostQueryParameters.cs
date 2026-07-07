namespace Application.DTOs.Blog.Admin;

public class AdminBlogPostQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? CategorySlug { get; set; }

    public bool? IsPublished { get; set; }
}