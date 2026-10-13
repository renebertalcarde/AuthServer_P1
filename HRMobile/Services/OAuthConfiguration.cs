namespace HRMobile.Services;

public static class OAuthConfiguration
{
    // Android emulator -> development computer.
    public const string Authority =
        "https://localhost:7284";

    public const string ClientId =
        "authserver-p1-mobile";

    public const string RedirectUri =
        "com.companyname.hrmobile.auth://callback";

    public const string Scope =
        "openid profile email";

    public const string DiscoveryEndpoint =
        Authority + "/.well-known/openid-configuration";
}