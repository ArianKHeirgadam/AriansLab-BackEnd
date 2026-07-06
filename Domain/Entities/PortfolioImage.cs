using Domain.Common;
namespace Domain.Entities
{
    public class PortfolioImage : SoftDeleteEntity
    {
        public Guid PortfolioId { get; set; }

        public string ImageUrl { get; set; } = string.Empty;

        public bool IsCover { get; set; }

        public int DisplayOrder { get; set; }

        public virtual Portfolio Portfolio { get; set; } = null!;
    }
}
