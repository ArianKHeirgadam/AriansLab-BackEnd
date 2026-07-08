namespace Application.DTOs.Services;

public class ServiceListItemDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Thumbnail { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public int EstimatedDeliveryDays { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public string? Icon { get; set; }
}