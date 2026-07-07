namespace Application.DTOs.Portfolio;

public class PortfolioImageDto
{
    public Guid Id { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsCover { get; set; }

    public int DisplayOrder { get; set; }
}