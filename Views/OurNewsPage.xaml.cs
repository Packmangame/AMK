using AMK.Services;

namespace AMK.Views;

public partial class OurNewsPage : ContentPage
{
	Models.Users.User _user;

    public OurNewsPage()
	{
		InitializeComponent();
		_user = SessionService.Instance.CurrentUser;
		
	}
}