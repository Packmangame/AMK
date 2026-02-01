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
            builder.Services.AddSingleton<INewsService>(sp =>
                sp.GetRequiredService<RssNewsServices>());                  

            builder.Services.AddTransient<NewsViewModel>();

            // MySQL (Timeweb Cloud)
            builder.Services.AddSingleton<IDatabaseService, MySqlDatabaseService>();
            builder.Services.AddSingleton<SessionService>(_ => SessionService.Instance);

            builder.Services.AddSingleton<ICompanyNews, MySqlCompanyNews>();
            builder.Services.AddTransient<CompanyNewsModel>();
            builder.Services.AddTransient<CompanyNewsListViewModel>();
            builder.Services.AddTransient<EditCompanyNewsViewModel>();
            builder.Services.AddTransient<CompanyNewsDetailViewModel>();
            builder.Services.AddTransient<UsersViewModel>();

            builder.Services.AddTransient<CalendarViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();
            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<EditDatabasePage>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<CalendarPage>();
            builder.Services.AddTransient<CompanyNewsPage>();
            builder.Services.AddTransient<EditCompanyNewsPage>();
            builder.Services.AddTransient<CompanyNewsDetailPage>();
            //builder.Services.AddTransient<CompanyNewsDetailPage>();
            //builder.Services.AddTransient<EditProfilePage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<NewsDetailPage>(); 

            return builder.Build();
        }
    }
}
