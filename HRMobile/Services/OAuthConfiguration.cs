namespace HRMobile.Services;

public static class OAuthConfiguration
{
    // Android emulator -> development computer.
    public const string Authority =
        "https://192.168.1.4:7284";

    public const string ClientId =
        "authserver-p1-mobile";

    public const string RedirectUri =
        "com.logixsys.hrmobile.auth://callback";

    public const string Scope =
        "openid profile email";

    public const string DiscoveryEndpoint =
        Authority + "/.well-known/openid-configuration";
}