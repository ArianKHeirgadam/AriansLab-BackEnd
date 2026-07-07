namespace Application.DTOs.Portfolio;

public class PortfolioDetailDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ShortDescription { get; set; }

    public string ClientName { get; set; } = string.Empty;

    public DateTime ProjectDate { get; set; }

    public string Thumbnail { get; set; } = string.Empty;

    public string WebsiteUrl { get; set; } = string.Empty;

    public string? GithubUrl { get; set; }

    public bool IsFeatured { get; set; }

    public int DisplayOrder { get; set; }

    public Guid CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string CategorySlug { get; set; } = string.Empty;

    public IReadOnlyList<PortfolioImageDto> Images { get; set; } =
        Array.Empty<PortfolioImageDto>();

    public IReadOnlyList<PortfolioTechnologyDto> Technologies { get; set; } =
        Array.Empty<PortfolioTechnologyDto>();
}