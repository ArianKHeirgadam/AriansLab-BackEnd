using Application.DTOs.Logs;

namespace Application.Interfaces;

public interface IAuditLogReadService
{
    Task<List<AuditLogDto>> GetAllAsync(
        Guid? userId = null,
        string? action = null,
        string? entityName = null,
        string? entityId = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<AuditLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}