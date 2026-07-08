using Application.DTOs.Services;

namespace Application.Interfaces;

public interface IServiceAdminService
{
    Task<List<AdminServiceDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminServiceDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminServiceDto> CreateAsync(
        CreateServiceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminServiceDto?> UpdateAsync(
        Guid id,
        UpdateServiceRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}