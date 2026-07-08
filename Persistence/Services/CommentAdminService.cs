using Application.DTOs.Comments;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class CommentAdminService : ICommentAdminService
{
    private readonly ApplicationDbContext _dbContext;

    public CommentAdminService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CommentDto>> GetAllAsync(
        Guid? blogPostId = null,
        bool? isApproved = null,
        int skip = 0,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 500);

        var query = _dbContext.Comments
            .AsNoTracking()
            .AsQueryable();

        if (blogPostId.HasValue)
        {
            query = query.Where(x => x.BlogPostId == blogPostId.Value);
        }

        if (isApproved.HasValue)
        {
            query = query.Where(x => x.IsApproved == isApproved.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new CommentDto
            {
                Id = x.Id,
                BlogPostId = x.BlogPostId,
                BlogPostTitle = x.BlogPost.Title,
                UserId = x.UserId,
                UserFullName = x.User != null ? x.User.FullName : null,
                UserEmail = x.User != null ? x.User.Email : null,
                ParentCommentId = x.ParentCommentId,
                FullName = x.FullName,
                Email = x.Email,
                Message = x.Message,
                IsApproved = x.IsApproved,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<CommentDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CommentDto
            {
                Id = x.Id,
                BlogPostId = x.BlogPostId,
                BlogPostTitle = x.BlogPost.Title,
                UserId = x.UserId,
                UserFullName = x.User != null ? x.User.FullName : null,
                UserEmail = x.User != null ? x.User.Email : null,
                ParentCommentId = x.ParentCommentId,
                FullName = x.FullName,
                Email = x.Email,
                Message = x.Message,
                IsApproved = x.IsApproved,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<CommentDto?> UpdateApprovalAsync(
        Guid id,
        UpdateCommentApprovalRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.Comments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comment is null)
        {
            return null;
        }

        comment.IsApproved = request.IsApproved;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(comment.Id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var comment = await _dbContext.Comments
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (comment is null)
        {
            return false;
        }

        _dbContext.Comments.Remove(comment);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}