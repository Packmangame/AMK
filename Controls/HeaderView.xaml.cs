using AMK.Views;

namespace AMK.Controls;

public partial class HeaderView : ContentView
{
	public HeaderView()
	{
		InitializeComponent();
	}
    private void MenuButton_Clicked(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = true;
    }

    private async void ProfileButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }
}