namespace Application.DTOs.Services;

public class AdminServiceDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Thumbnail { get; set; } = string.Empty;

    public string CoverImage { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string Description { get; set; } = string.Empty;

    public int EstimatedDeliveryDays { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public string? Icon { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<AdminServiceFeatureDto> Features { get; set; } =
        Array.Empty<AdminServiceFeatureDto>();
}