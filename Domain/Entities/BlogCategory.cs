using Domain.Common;
namespace Domain.Entities
{
    public class BlogCategory : SoftDeleteEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}

