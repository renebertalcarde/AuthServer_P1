namespace SharedLib.OAuthConfiguration;

public abstract class OAuthConfigurationBase : IOAuthConfiguration
{
    public abstract string Authority { get; }
    public abstract string ClientId { get; }
    public abstract string RedirectUri { get; }
    public string Scope => "openid profile email";
    public string DiscoveryEndpoint => Authority + "/.well-known/openid-configuration";

}