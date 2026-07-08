namespace Application.DTOs.Pricing;

public class UpdatePricingPlanRequestDto
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int Duration { get; set; }

    public int DeliveryDays { get; set; }

    public bool IsPopular { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public List<UpdatePlanFeatureRequestDto> Features { get; set; } = new();
}

public class UpdatePlanFeatureRequestDto
{
    public string Feature { get; set; } = string.Empty;
}