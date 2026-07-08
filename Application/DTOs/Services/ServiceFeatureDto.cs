namespace Application.DTOs.Services;

public class ServiceFeatureDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}