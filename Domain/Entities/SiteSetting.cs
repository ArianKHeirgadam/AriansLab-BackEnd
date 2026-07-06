using Domain.Common;

namespace Domain.Entities
{
    public class SiteSetting : SoftDeleteEntity
    {
        public string SiteName { get; set; } = string.Empty;

        public string? Logo { get; set; }

        public string? DarkLogo { get; set; }

        public string? Favicon { get; set; }

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string FooterText { get; set; } = string.Empty;

        public string Copyright { get; set; } = string.Empty;

        public string? GoogleMap { get; set; }

        public string? GoogleAnalytics { get; set; }

        public string MetaTitle { get; set; } = string.Empty;

        public string MetaDescription { get; set; } = string.Empty;

        public string MetaKeywords { get; set; } = string.Empty;
    }
}
