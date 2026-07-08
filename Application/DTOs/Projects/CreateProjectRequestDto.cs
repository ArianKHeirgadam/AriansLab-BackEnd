using Domain.Enums;

namespace Application.DTOs.Projects;

public class CreateProjectRequestDto
{
    public Guid UserId { get; set; }

    public Guid PricingPlanId { get; set; }

    public string? ProjectCode { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

    public byte Progress { get; set; }

    public decimal Price { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? AdminNote { get; set; }

    public string? CustomerComment { get; set; }
}