namespace Application.DTOs.HeroSections;

public class UpdateHeroSectionRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Subtitle { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string PrimaryButtonText { get; set; } = string.Empty;

    public string PrimaryButtonUrl { get; set; } = string.Empty;

    public string? SecondaryButtonText { get; set; }

    public string? SecondaryButtonUrl { get; set; }

    public string? BackgroundImage { get; set; }

    public string? VideoUrl { get; set; }

    public bool IsActive { get; set; }
}