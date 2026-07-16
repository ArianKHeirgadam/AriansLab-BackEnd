using Application.DTOs.Comments;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class CommentReadService : ICommentReadService
{
    private readonly ApplicationDbContext _dbContext;

    public CommentReadService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<PublicCommentDto>> GetApprovedByBlogPostIdAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.BlogPostId == blogPostId && x.IsApproved)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new PublicCommentDto
            {
                Id = x.Id,
                BlogPostId = x.BlogPostId,
                ParentCommentId = x.ParentCommentId,
                FullName = x.FullName,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PublicCommentDto?> GetApprovedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsApproved)
            .Select(x => new PublicCommentDto
            {
                Id = x.Id,
                BlogPostId = x.BlogPostId,
                ParentCommentId = x.ParentCommentId,
                FullName = x.FullName,
                Message = x.Message,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
