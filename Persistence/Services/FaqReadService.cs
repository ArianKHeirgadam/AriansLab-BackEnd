using Application.DTOs.FAQs;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class FaqReadService : IFaqReadService
{
    private readonly ApplicationDbContext _dbContext;

    public FaqReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FaqDto>> GetActiveFaqsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FAQs
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Question)
            .Select(x => new FaqDto
            {
                Id = x.Id,
                Question = x.Question,
                Answer = x.Answer,
                DisplayOrder = x.DisplayOrder
            })
            .ToListAsync(cancellationToken);
    }
}