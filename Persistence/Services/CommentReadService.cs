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

    public async Task<List<CommentDto>> GetApprovedByBlogPostIdAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.BlogPostId == blogPostId && x.IsApproved)
            .OrderBy(x => x.CreatedAt)
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

    public async Task<CommentDto?> GetApprovedByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.Id == id && x.IsApproved)
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
}