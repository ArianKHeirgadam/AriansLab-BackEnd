using Domain.Common;
namespace Domain.Entities
{
    public class Portfolio : SoftDeleteEntity
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string ClientName { get; set; } = string.Empty;

        public DateTime ProjectDate { get; set; }
        public string Thumbnail { get; set; } = string.Empty;

        public string? GithubUrl { get; set; }

        public string? ShortDescription { get; set; }

        public int DisplayOrder { get; set; }

        public Guid PortfolioCategoryId { get; set; }

        public virtual PortfolioCategory Category { get; set; } = null!;

        public ICollection<PortfolioTechnology> Technologies { get; set; } = new List<PortfolioTechnology>();

        public string WebsiteUrl { get; set; } = string.Empty;

        public bool IsFeatured { get; set; }

        public ICollection<PortfolioImage> Images { get; set; } = new List<PortfolioImage>();
    }
}
