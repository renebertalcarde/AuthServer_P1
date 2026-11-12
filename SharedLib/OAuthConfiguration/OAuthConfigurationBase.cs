namespace SharedLib.OAuthConfiguration;

public abstract class OAuthConfigurationBase : IOAuthConfiguration
{
    public abstract string Authority { get; }
    public abstract string ClientId { get; }
    public abstract string SignIn_RedirectUri { get; }
    public abstract string SignOut_RedirectUri { get; }
    public string Scope => "openid profile email";
    public string DiscoveryEndpoint => Authority + "/.well-known/openid-configuration";
    public string EndSessionEndpoint { get; set; } = string.Empty;
}