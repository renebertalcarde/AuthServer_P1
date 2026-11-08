namespace SharedLib.OAuthConfiguration;

public interface IOAuthConfiguration
{
    string Authority { get; }

    string ClientId { get; }

    string RedirectUri { get; }

    string Scope { get; }

    string DiscoveryEndpoint { get; }
}