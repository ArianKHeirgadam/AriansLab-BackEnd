using Application.DTOs.Blog;



namespace Application.Interfaces;

public interface IBlogCategoryReadService
{
    Task<List<BlogCategoryDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<BlogCategoryDto?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);
}