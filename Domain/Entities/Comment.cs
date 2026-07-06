using Domain.Common;

namespace Domain.Entities
{
    public class Comment : SoftDeleteEntity
    {
        public Guid BlogPostId { get; set; }

        public Guid? UserId { get; set; }

        public Guid? ParentCommentId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public bool IsApproved { get; set; }

        public virtual BlogPost BlogPost { get; set; } = null!;

        public virtual User? User { get; set; }

        public virtual Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();
    }
}
