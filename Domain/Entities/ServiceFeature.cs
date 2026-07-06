using Domain.Common;
namespace Domain.Entities
{
    public class ServiceFeature : SoftDeleteEntity
    {
        public Guid ServiceId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public virtual Service Service { get; set; } = null!;
    }
}
