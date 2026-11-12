namespace SharedLib.OAuthConfiguration;

public class Android_Emulator : Android
{
    public override string Authority => "https://10.0.2.2:7284";

    public override string ClientId => "authserver-p1-mobile";

    public override string SignIn_RedirectUri => "com.hrmobile://signin-callback";
    public override string SignOut_RedirectUri => "com.hrmobile://signout-callback";
}