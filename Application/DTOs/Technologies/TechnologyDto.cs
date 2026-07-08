namespace Application.DTOs.Technologies;

public class TechnologyDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public string? Color { get; set; }

    public int PortfolioCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}