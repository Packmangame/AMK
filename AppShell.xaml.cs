using AMK.Views;

namespace AMK
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(NewsDetailPage), typeof(NewsDetailPage));
            Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
            Routing.RegisterRoute(nameof(OurNewsPage), typeof(OurNewsPage));
            //Routing.RegisterRoute(nameof(EducationPage), typeof(EducationPage));
            Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
            FlyoutBehavior = FlyoutBehavior.Disabled;
            CurrentItem = LoginPageTab;
            
        }
        public void EnableMenu()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FlyoutBehavior = FlyoutBehavior.Flyout;

                var flyoutItems = Items.Where(x => x is FlyoutItem).Cast<FlyoutItem>();
                foreach (var item in flyoutItems)
                {
                    item.IsVisible = true;
                }
            });
        }
    }
}
