using Domain.Common;
namespace Domain.Entities
{
    public class Technology : SoftDeleteEntity
    {
        public string Name { get; set; } = string.Empty;

        public string? Icon { get; set; }

        public string? Color { get; set; }

        public ICollection<PortfolioTechnology> Portfolios { get; set; } = new List<PortfolioTechnology>();
    }
}
