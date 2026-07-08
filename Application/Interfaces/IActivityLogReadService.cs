using Application.DTOs.Logs;

namespace Application.Interfaces;

public interface IActivityLogReadService
{
    Task<List<ActivityLogDto>> GetAllAsync(
        Guid? userId = null,
        string? activity = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<ActivityLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}