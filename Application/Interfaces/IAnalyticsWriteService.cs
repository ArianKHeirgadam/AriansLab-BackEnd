using Application.DTOs.Analytics;

namespace Application.Interfaces;

public interface IAnalyticsWriteService
{
    Task TrackPageViewAsync(
        TrackPageViewRequestDto request,
        string? userAgent,
        CancellationToken cancellationToken = default);
}
