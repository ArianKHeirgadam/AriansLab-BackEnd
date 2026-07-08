using Domain.Enums;

namespace Application.DTOs.Projects;

public class UpdateProjectStatusRequestDto
{
    public ProjectStatus Status { get; set; }

    public byte Progress { get; set; }

    public string? AdminNote { get; set; }
}