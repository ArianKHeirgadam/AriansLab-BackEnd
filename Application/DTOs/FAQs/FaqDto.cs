namespace Application.DTOs.FAQs;

public class FaqDto
{
    public Guid Id { get; set; }

    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}