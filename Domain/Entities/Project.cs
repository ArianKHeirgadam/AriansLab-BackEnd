using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Project : SoftDeleteEntity
{
    public Guid UserId { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public Guid PricingPlanId { get; set; }

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

    public virtual User User { get; set; } = null!;

    public virtual PricingPlan PricingPlan { get; set; } = null!;

    public ICollection<ProjectFile> Files { get; set; } = new List<ProjectFile>();

    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}