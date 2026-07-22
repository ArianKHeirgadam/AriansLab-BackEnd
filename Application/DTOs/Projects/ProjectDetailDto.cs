using Domain.Enums;

namespace Application.DTOs.Projects;

public class ProjectDetailDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string CustomerFullName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public Guid PricingPlanId { get; set; }

    public string PricingPlanTitle { get; set; } = string.Empty;

    public string ProjectCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public byte Progress { get; set; }

    public decimal Price { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? AdminNote { get; set; }

    public string? CustomerComment { get; set; }

    public bool IsCustomerCommentApproved { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
