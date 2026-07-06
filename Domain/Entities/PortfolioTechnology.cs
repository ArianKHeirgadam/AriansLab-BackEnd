using Domain.Common;
namespace Domain.Entities
{
    public class PortfolioTechnology : SoftDeleteEntity
    {
        public Guid PortfolioId { get; set; }

        public Guid TechnologyId { get; set; }

        public virtual Portfolio Portfolio { get; set; } = null!;

        public virtual Technology Technology { get; set; } = null!;
    }
}
