namespace Application.DTOs.HeroSections;

public class HeroSectionDto
{
    public Guid Id { get; set; }

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

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}