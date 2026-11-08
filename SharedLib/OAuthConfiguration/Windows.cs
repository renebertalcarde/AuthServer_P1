namespace SharedLib.OAuthConfiguration;

public class Windows : OAuthConfigurationBase
{
    public override string Authority => "https://localhost:7284";

    public override string ClientId => "authserver-p1-windows";

    public override string RedirectUri => "http://127.0.0.1:49152/callback";
}