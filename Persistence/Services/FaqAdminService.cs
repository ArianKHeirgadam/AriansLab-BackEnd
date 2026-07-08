using Application.DTOs.FAQs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class FaqAdminService : IFaqAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public FaqAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<AdminFaqDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FAQs
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Question)
            .Select(x => new AdminFaqDto
            {
                Id = x.Id,
                Question = x.Question,
                Answer = x.Answer,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<AdminFaqDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.FAQs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AdminFaqDto
            {
                Id = x.Id,
                Question = x.Question,
                Answer = x.Answer,
                DisplayOrder = x.DisplayOrder,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<AdminFaqDto> CreateAsync(
        CreateFaqRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Question, request.Answer);

        var faq = new FAQ
        {
            Question = request.Question.Trim(),
            Answer = request.Answer.Trim(),
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _dbContext.FAQs.AddAsync(faq, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdFaq = await GetByIdAsync(faq.Id, cancellationToken);

        return createdFaq!;
    }

    public async Task<AdminFaqDto?> UpdateAsync(
        Guid id,
        UpdateFaqRequestDto request,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request.Question, request.Answer);

        var faq = await _dbContext.FAQs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faq is null)
        {
            return null;
        }

        faq.Question = request.Question.Trim();
        faq.Answer = request.Answer.Trim();
        faq.DisplayOrder = request.DisplayOrder;
        faq.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(faq.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var faq = await _dbContext.FAQs
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (faq is null)
        {
            return false;
        }

        _dbContext.FAQs.Remove(faq);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static void ValidateRequest(string question, string answer)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new InvalidOperationException("FAQ question is required.");
        }

        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new InvalidOperationException("FAQ answer is required.");
        }
    }
}