namespace Application.DTOs.SocialMedias;

public class CreateSocialMediaRequestDto
{
    public string Platform { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}