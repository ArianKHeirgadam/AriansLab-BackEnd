namespace Application.DTOs.Portfolio;

public class PortfolioListItemDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string Thumbnail { get; set; } = string.Empty;

    public string ClientName { get; set; } = string.Empty;

    public DateTime ProjectDate { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategorySlug { get; set; } = string.Empty;

    public IReadOnlyList<PortfolioTechnologyDto> Technologies { get; set; } =
        Array.Empty<PortfolioTechnologyDto>();
}