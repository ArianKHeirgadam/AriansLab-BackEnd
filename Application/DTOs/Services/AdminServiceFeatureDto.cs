namespace Application.DTOs.Services;

public class AdminServiceFeatureDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}