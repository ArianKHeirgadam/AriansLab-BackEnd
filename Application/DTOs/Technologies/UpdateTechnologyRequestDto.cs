namespace Application.DTOs.Technologies;

public class UpdateTechnologyRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public string? Color { get; set; }
}