using AMK.Services;
using AMK.Views;
using AMK.Models.ViewModels;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace AMK
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
                    fonts.AddFont("benzin-bold.ttf", "BenzinBold");
                    //fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    //fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.UseMauiApp<App>().UseMauiCommunityToolkit();
#if DEBUG
            builder.Logging.AddDebug();
#endif
            
            builder.Services.AddSingleton<RssNewsServices>();
            builder.Services.AddSingleton<NewsViewModel>();
            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<EditDatabase>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<OurNewsPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<NewsDetailPage>(); 

            return builder.Build();
        }
    }
}
