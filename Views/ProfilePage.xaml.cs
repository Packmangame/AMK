namespace AMK.Views;

public partial class ProfilePage : ContentPage
{
	public ProfilePage()
	{
		InitializeComponent();
	}

    private void EditProfile(object sender, EventArgs e)
    {

    }

    private async void EditBd(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(EditDatabase)}");
    }
}