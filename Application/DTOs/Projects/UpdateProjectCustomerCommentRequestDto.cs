using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Projects;

public class UpdateProjectCustomerCommentRequestDto
{
    [StringLength(3000)]
    public string? CustomerComment { get; set; }
}
