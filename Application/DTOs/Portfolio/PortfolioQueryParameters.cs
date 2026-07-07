namespace Application.DTOs.Portfolio;

public class PortfolioQueryParameters
{
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? CategorySlug { get; set; }

    public bool? IsFeatured { get; set; }
}