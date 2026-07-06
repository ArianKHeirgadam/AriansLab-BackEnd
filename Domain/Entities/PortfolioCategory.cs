using Domain.Common;
namespace Domain.Entities
{
    public class PortfolioCategory : SoftDeleteEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<Portfolio> Portfolios { get; set; } = new List<Portfolio>();
    }
}
