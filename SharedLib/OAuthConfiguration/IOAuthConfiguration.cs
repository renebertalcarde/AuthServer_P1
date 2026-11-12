namespace SharedLib.OAuthConfiguration;

public interface IOAuthConfiguration
{
    string Authority { get; }

    string ClientId { get; }

    string SignIn_RedirectUri { get; }
    string SignOut_RedirectUri { get; }
    string Scope { get; }

    string DiscoveryEndpoint { get; }
    string EndSessionEndpoint { get; set; }
}