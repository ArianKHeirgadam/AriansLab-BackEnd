namespace Application.DTOs.Blog.Admin;

public class UpdateBlogCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}