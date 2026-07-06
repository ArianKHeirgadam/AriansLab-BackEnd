using Domain.Common;
namespace Domain.Entities
{
    public class BlogPost : SoftDeleteEntity
    {
        public Guid AuthorId { get; set; }

        public Guid CategoryId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Excerpt { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string CoverImage { get; set; } = string.Empty;

        public int ReadTime { get; set; }

        public int ViewCount { get; set; }

        public bool IsPublished { get; set; }
        public string? SeoTitle { get; set; }

        public string? SeoDescription { get; set; }

        public string? Keywords { get; set; }

        public DateTime? PublishedAt { get; set; }

        public virtual User Author { get; set; } = null!;

        public virtual BlogCategory Category { get; set; } = null!;
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
