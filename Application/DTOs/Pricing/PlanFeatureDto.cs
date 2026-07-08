namespace Application.DTOs.Pricing;

public class PlanFeatureDto
{
    public Guid Id { get; set; }

    public string Feature { get; set; } = string.Empty;
}