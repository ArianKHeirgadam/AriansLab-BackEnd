namespace Application.DTOs.Portfolio;

public class PortfolioCategoryDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Slug { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public int PortfolioCount { get; set; }
}