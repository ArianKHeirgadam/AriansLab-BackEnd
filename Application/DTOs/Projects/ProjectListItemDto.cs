using Domain.Enums;

namespace Application.DTOs.Projects;

public class ProjectListItemDto
{
    public Guid Id { get; set; }

    public string ProjectCode { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public ProjectStatus Status { get; set; }

    public byte Progress { get; set; }

    public decimal Price { get; set; }

    public decimal PaidAmount { get; set; }

    public DateTime? EstimatedDeliveryDate { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? CustomerComment { get; set; }

    public bool IsCustomerCommentApproved { get; set; }

    public DateTime CreatedAt { get; set; }
}
