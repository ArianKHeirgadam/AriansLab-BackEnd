using Application.DTOs.EmailTemplates;

namespace Application.Interfaces;

public interface IEmailTemplateAdminService
{
    Task<List<EmailTemplateDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<List<EmailTemplateDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<EmailTemplateDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<EmailTemplateDto?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    Task<EmailTemplateDto> CreateAsync(
        CreateEmailTemplateRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EmailTemplateDto?> UpdateAsync(
        Guid id,
        UpdateEmailTemplateRequestDto request,
        CancellationToken cancellationToken = default);

    Task<EmailTemplateDto?> UpdateActiveStatusAsync(
        Guid id,
        UpdateEmailTemplateActiveStatusRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}