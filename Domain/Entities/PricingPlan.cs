using Domain.Common;
namespace Domain.Entities
{
    public class PricingPlan : SoftDeleteEntity
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Duration { get; set; }
        public bool IsPopular { get; set; }

        public int DisplayOrder { get; set; }
        public int DeliveryDays { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<PlanFeature> Features { get; set; } = new List<PlanFeature>();

        public ICollection<Project> Projects { get; set; } = new List<Project>();
    }
}
