using HRMobile.Services;
using Microsoft.Extensions.Logging;

namespace HRMobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<IAuthService, AuthService>();

            builder.Services.AddHttpClient<IAuthService, AuthService>();
            builder.Services.AddTransient<MainPage>();

            return builder.Build();
        }
    }
}
