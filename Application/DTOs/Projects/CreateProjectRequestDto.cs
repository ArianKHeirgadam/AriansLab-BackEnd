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

    /// <summary>
    /// Creates the first provisional invoice atomically with the project.
    /// Existing API clients keep the old behavior unless they opt in.
    /// </summary>
    public bool CreateInitialInvoice { get; set; }

    public DateTime? InvoiceDueDate { get; set; }

    public decimal InvoiceDiscountAmount { get; set; }

    public decimal InvoiceTaxAmount { get; set; }

    public string? InvoiceDescription { get; set; }
}
