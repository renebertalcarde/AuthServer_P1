using Android.App;
using Android.Content;
using Android.Content.PM;
using WebAuthenticatorCallbackActivityBase = Microsoft.Maui.Authentication.WebAuthenticatorCallbackActivity;

namespace HRMobile;

[Activity(
    NoHistory = true,
    LaunchMode = LaunchMode.SingleTop,
    Exported = true)]
[IntentFilter(
    new[] { Intent.ActionView },
    Categories = new[]
    {
        Intent.CategoryDefault,
        Intent.CategoryBrowsable
    },
    DataScheme = "com.logixsys.hrmobile.auth",
    DataHost = "callback")]
public class WebAuthenticatorCallbackActivity
    : WebAuthenticatorCallbackActivityBase
{
}