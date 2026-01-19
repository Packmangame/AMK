using AMK.Models.Users;
using AMK.Services;
using Microsoft.Maui.Controls;
using static System.Net.Mime.MediaTypeNames;

namespace AMK.Views;

public partial class ProfilePage : ContentPage
{
    User _user;
	public ProfilePage()
	{
		InitializeComponent();
        _user = SessionService.Instance.CurrentUser;
        if (_user.Role == 1)
        {
            
        }
	}

    private void EditProfile(object sender, EventArgs e)
    {

    }

    private async void EditBd(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(EditDatabase)}");
    }
}