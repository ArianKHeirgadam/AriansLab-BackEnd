using Domain.Common;
namespace Domain.Entities
{
    public class Setting : SoftDeleteEntity
    {
        public string SiteName { get; set; } = string.Empty;

        public string Logo { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Telegram { get; set; } = string.Empty;

        public string Instagram { get; set; } = string.Empty;

        public string Linkedin { get; set; } = string.Empty;

        public string WhatsApp { get; set; } = string.Empty;

        public string FooterDescription { get; set; } = string.Empty;
    }
}
