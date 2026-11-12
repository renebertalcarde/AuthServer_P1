namespace SharedLib.OAuthConfiguration;

public class Windows : OAuthConfigurationBase
{
    public override string Authority => "https://localhost:7284";

    public override string ClientId => "authserver-p1-windows";

    public override string SignIn_RedirectUri => "com.hrmobile://signin-callback";
    public override string SignOut_RedirectUri => "com.hrmobile://signout-callback";
}