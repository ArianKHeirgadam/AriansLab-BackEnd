using Application.DTOs.FAQs;

namespace Application.Interfaces;

public interface IFaqAdminService
{
    Task<List<AdminFaqDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminFaqDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminFaqDto> CreateAsync(
        CreateFaqRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminFaqDto?> UpdateAsync(
        Guid id,
        UpdateFaqRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}