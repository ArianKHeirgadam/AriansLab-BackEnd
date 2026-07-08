namespace Application.DTOs.Pricing;

public class AdminPricingPlanDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Duration { get; set; }

    public int DeliveryDays { get; set; }

    public bool IsPopular { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<PlanFeatureDto> Features { get; set; } =
        Array.Empty<PlanFeatureDto>();
}