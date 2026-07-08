namespace Application.DTOs.Technologies;

public class CreateTechnologyRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string? Icon { get; set; }

    public string? Color { get; set; }
}