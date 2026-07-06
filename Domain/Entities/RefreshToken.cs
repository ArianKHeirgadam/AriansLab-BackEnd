using Domain.Common;
namespace Domain.Entities
{
    public class RefreshToken : AuditableEntity
    {
        public Guid UserId { get; set; }

        public string Token { get; set; } = string.Empty;

        public DateTime ExpireDate { get; set; }

        public bool IsRevoked { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
