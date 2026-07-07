using Application.DTOs.Common;
using Application.DTOs.Portfolio;

namespace Application.Interfaces;

public interface IPortfolioReadService
{
    Task<PagedResultDto<PortfolioListItemDto>> GetPortfoliosAsync(
        PortfolioQueryParameters parameters,
        CancellationToken cancellationToken = default);

    Task<PortfolioDetailDto?> GetPortfolioBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PortfolioCategoryDto>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);
}