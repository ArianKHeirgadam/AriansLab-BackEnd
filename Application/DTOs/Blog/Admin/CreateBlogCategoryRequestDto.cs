namespace Application.DTOs.Blog.Admin;

public class CreateBlogCategoryRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;
}