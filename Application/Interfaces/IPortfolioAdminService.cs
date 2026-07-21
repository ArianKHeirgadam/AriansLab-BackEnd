using Application.DTOs.Portfolio;

namespace Application.Interfaces;

public interface IPortfolioAdminService
{
    Task<List<AdminPortfolioDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<AdminPortfolioDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AdminPortfolioDto> CreateAsync(
        CreatePortfolioRequestDto request,
        CancellationToken cancellationToken = default);

    Task<AdminPortfolioDto?> UpdateAsync(
        Guid id,
        UpdatePortfolioRequestDto request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
