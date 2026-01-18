using AMK.Views;

namespace AMK
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            //Routing.RegisterRoute(nameof(LoginPage), typeof(LoginPage));
            //Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            //Routing.RegisterRoute(nameof(NewsDetailPage), typeof(NewsDetailPage));
            //Routing.RegisterRoute(nameof(CalendarPage), typeof(CalendarPage));
            //Routing.RegisterRoute(nameof(OurNewsPage), typeof(OurNewsPage));
            ////Routing.RegisterRoute(nameof(EducationPage), typeof(EducationPage));
            //Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));


            FlyoutBehavior = FlyoutBehavior.Disabled;
            CurrentItem = LoginPageTab;



        }
        private void MenuButton_Clicked(object sender, EventArgs e)
        {
          FlyoutIsPresented = true;
        }
        public void EnableMenu()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Включить Flyout меню
                FlyoutBehavior = FlyoutBehavior.Flyout;

                var flyoutItems = Items.Where(x => x is FlyoutItem).Cast<FlyoutItem>();
                foreach (var item in flyoutItems)
                {
                    item.IsVisible = true;
                }
            });
        }
        private async void ProfileButton_Clicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(ProfilePage)}");
        }
    }
}
