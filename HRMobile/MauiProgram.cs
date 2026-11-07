using HRMobile.Components;
using HRMobile.Objects;
using HRMobile.Pages;
using HRMobile.Services;
using HRMobile.ViewModels;
using Microsoft.Extensions.Logging;

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
        builder.Services.AddSingleton<IAuthService>(provider => provider.GetRequiredService<AuthService>());
        builder.Services.AddSingleton<UserViewModel>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<AppHeader>();
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<Employees>();
        return builder.Build();
    }
}