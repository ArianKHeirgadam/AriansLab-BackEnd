using Domain.Common;
namespace Domain.Entities
{
    public class PlanFeature : SoftDeleteEntity
    {
        public Guid PricingPlanId { get; set; }

        public string Feature { get; set; } = string.Empty;

        public virtual PricingPlan PricingPlan { get; set; } = null!;
    }
}
