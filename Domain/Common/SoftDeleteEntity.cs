namespace Domain.Common
{
    public abstract class SoftDeleteEntity : AuditableEntity
    {
        public DateTime? DeletedAt { get; set; }

        public Guid? DeletedBy { get; set; }
    }
}
