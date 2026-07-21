namespace AriansLab.Api.Security;

public sealed class AuthCookieSettings
{
    public const string SectionName = "AuthCookies";
    public const string AccessCookieName = "__Host-AriansLab.Access";
    public const string RefreshCookieName = "__Secure-AriansLab.Refresh";
    public const string RememberCookieName = "__Host-AriansLab.Remember";
    public const string AntiforgeryCookieName = "__Host-AriansLab.Csrf";

    public bool Secure { get; set; } = true;

    public string SameSite { get; set; } = "Lax";

    public bool Partitioned { get; set; }
}
