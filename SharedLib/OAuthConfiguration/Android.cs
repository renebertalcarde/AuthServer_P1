namespace SharedLib.OAuthConfiguration;

public class Android : OAuthConfigurationBase
{
    public override string Authority => "https://192.168.68.103:7284";

    public override string ClientId => "authserver-p1-mobile";

    public override string SignIn_RedirectUri => "com.hrmobile://signin-callback";
    public override string SignOut_RedirectUri => "com.hrmobile://signout-callback";
}