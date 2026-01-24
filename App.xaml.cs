using AMK.Models.ViewModels;
using AMK.Services;
using AMK.Views;

namespace AMK
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;
        public static NewsViewModel NewsViewModel { get; private set; }
        public App(IServiceProvider services)
        {
            InitializeComponent();
            SQLitePCL.Batteries_V2.Init();
            _services = services;
            var localizationService = LocalizationService.Instance;
           
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {

            var shell = _services.GetRequiredService<AppShell>();
            return new Window(shell);
            //Task.Run(async () =>
            //{
            //    await Task.Delay(1000); 

            //});
            //var newsService = Handler.MauiContext.Services.GetRequiredService<RssNewsServices>();
            //NewsViewModel = new NewsViewModel(newsService);

            //var mainPage = Handler.MauiContext.Services.GetRequiredService<MainPage>();
            //return new Window(new AppShell());
        }
    }
}