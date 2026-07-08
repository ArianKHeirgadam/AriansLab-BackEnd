namespace Application.DTOs.Services;

public class CreateServiceRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string Thumbnail { get; set; } = string.Empty;

    public string CoverImage { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string Description { get; set; } = string.Empty;

    public int EstimatedDeliveryDays { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public string? Icon { get; set; }

    public bool IsActive { get; set; } = true;

    public List<CreateServiceFeatureRequestDto> Features { get; set; } = new();
}

public class CreateServiceFeatureRequestDto
{
    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}