using Domain.Common;
namespace Domain.Entities
{
    public class Service : SoftDeleteEntity
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;

        public string CoverImage { get; set; } = string.Empty;

        public string? ShortDescription { get; set; }

        public int EstimatedDeliveryDays { get; set; }

        public bool IsFeatured { get; set; }

        public int DisplayOrder { get; set; }


        public string Description { get; set; } = string.Empty;

        public string? Icon { get; set; }


        public bool IsActive { get; set; } = true;

        public ICollection<ServiceFeature> Features { get; set; } = new List<ServiceFeature>();
    }
}
