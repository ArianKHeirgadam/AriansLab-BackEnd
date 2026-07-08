namespace Application.DTOs.EmailTemplates;

public class CreateEmailTemplateRequestDto
{
    public string Name { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}