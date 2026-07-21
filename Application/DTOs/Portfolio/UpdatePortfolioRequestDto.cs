namespace Application.DTOs.Portfolio;

public class UpdatePortfolioRequestDto
{
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
}
