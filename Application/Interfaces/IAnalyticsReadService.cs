using Application.DTOs.Analytics;

namespace Application.Interfaces;

public interface IAnalyticsReadService
{
    Task<AnalyticsDashboardDto> GetDashboardAsync(
        int days,
        CancellationToken cancellationToken = default);
}
