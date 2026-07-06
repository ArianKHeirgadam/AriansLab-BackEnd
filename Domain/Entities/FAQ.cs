using Domain.Common;
namespace Domain.Entities
{
    public class FAQ : SoftDeleteEntity
    {
        public string Question { get; set; } = string.Empty;

        public string Answer { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
