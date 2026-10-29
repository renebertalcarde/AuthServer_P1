using HRMobile.Pages;
using HRMobile.Services;
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
        builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<AuthService>());
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<EmployeesPage>();
        builder.Services.AddTransient<ProfilePage>();
        return builder.Build();
    }
}