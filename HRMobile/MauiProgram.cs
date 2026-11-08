using HRMobile.Components;
using HRMobile.Pages;
using HRMobile.Services;
using HRMobile.Services.Platforms;
using HRMobile.ViewModels;
using Microsoft.Extensions.Logging;
using SharedLib.OAuthConfiguration;

namespace HRMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        
        builder.Services.AddHttpClient<AuthService>();

#if  ANDROID
        builder.Services.AddSingleton<IPlatformAuthBrowser, AndroidAuthBrowser>();
        builder.Services.AddSingleton<IOAuthConfiguration, SharedLib.OAuthConfiguration.Android>();
#elif WINDOWS
        builder.Services.AddSingleton<IPlatformAuthBrowser, WindowsAuthBrowser>();
        builder.Services.AddSingleton<IOAuthConfiguration, SharedLib.OAuthConfiguration.Windows>();
#endif

        builder.Services.AddSingleton<IAuthService>(provider => provider.GetRequiredService<AuthService>());

        builder.Services.AddSingleton<UserViewModel>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<AppHeader>();
        builder.Services.AddTransient<AppPageLayout>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<Employees>();
        return builder.Build();
    }
}