using Application.DTOs.Comments;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Services;

public class CommentSubmitService : ICommentSubmitService
{
    private readonly ApplicationDbContext _dbContext;

    public CommentSubmitService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CommentDto> CreateAsync(
        CreateCommentRequestDto request,
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        ValidateComment(
            request.BlogPostId,
            request.FullName,
            request.Email,
            request.Message
        );

        var blogPostExists = await _dbContext.BlogPosts
            .AnyAsync(x => x.Id == request.BlogPostId, cancellationToken);

        if (!blogPostExists)
        {
            throw new InvalidOperationException("Blog post was not found.");
        }

        if (request.ParentCommentId.HasValue)
        {
            var parentExists = await _dbContext.Comments
                .AnyAsync(
                    x => x.Id == request.ParentCommentId.Value &&
                         x.BlogPostId == request.BlogPostId,
                    cancellationToken
                );

            if (!parentExists)
            {
                throw new InvalidOperationException("Parent comment was not found.");
            }
        }

        Guid? validUserId = null;

        if (userId.HasValue && userId.Value != Guid.Empty)
        {
            var userExists = await _dbContext.Users
                .AnyAsync(x => x.Id == userId.Value, cancellationToken);

            if (userExists)
            {
                validUserId = userId.Value;
            }
        }

        var comment = new Comment
        {
            BlogPostId = request.BlogPostId,
            UserId = validUserId,
            ParentCommentId = request.ParentCommentId,
            FullName = request.FullName.Trim(),
            Email = request.Email.Trim(),
            Message = request.Message.Trim(),
            IsApproved = false
        };

        await _dbContext.Comments.AddAsync(comment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var createdComment = await _dbContext.Comments
            .AsNoTracking()
            .Where(x => x.Id == comment.Id)
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
            .FirstAsync(cancellationToken);

        return createdComment;
    }

    private static void ValidateComment(
        Guid blogPostId,
        string fullName,
        string email,
        string message)
    {
        if (blogPostId == Guid.Empty)
        {
            throw new InvalidOperationException("Blog post id is required.");
        }

        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new InvalidOperationException("Full name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("Email is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("Message is required.");
        }

        if (!email.Contains('@'))
        {
            throw new InvalidOperationException("Email is invalid.");
        }
    }
}