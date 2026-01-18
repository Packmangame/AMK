using AMK.Models.Data;
using AMK.Models.ViewModels;
using AMK.Services;
using AMK.Views;

namespace AMK
{
    public partial class App : Application
    {
        public static NewsViewModel NewsViewModel { get; private set; }
        public App()
        {
            InitializeComponent();
            SQLitePCL.Batteries_V2.Init();
            var localizationService = LocalizationService.Instance;
           
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000); 
                                        
            });
            var newsService = Handler.MauiContext.Services.GetRequiredService<RssNewsServices>();
            NewsViewModel = new NewsViewModel(newsService);

            var mainPage = Handler.MauiContext.Services.GetRequiredService<MainPage>();
            return new Window(new AppShell());
        }
    }
}