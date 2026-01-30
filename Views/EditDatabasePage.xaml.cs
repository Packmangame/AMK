using AMK.Models.ViewModels;

namespace AMK.Views;

public partial class EditDatabasePage : ContentPage
{
	public EditDatabasePage(UsersViewModel usersViewModel)
	{
		InitializeComponent();
		BindingContext = usersViewModel;
	}
    
}