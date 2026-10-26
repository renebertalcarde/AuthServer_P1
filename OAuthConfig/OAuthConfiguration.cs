namespace OAuthConfig;

public static class OAuthConfiguration
{
    // Android emulator -> development computer.
    public const string Authority = 
        //"https://10.0.2.2:7284";
    "https://192.168.68.103:7284";

    public const string ClientId =
        "authserver-p1-mobile";

    public const string RedirectUri =
        "com.hrmobile.auth://callback";

    public const string Scope =
        "openid profile email";

    public const string DiscoveryEndpoint =
        Authority + "/.well-known/openid-configuration";
}