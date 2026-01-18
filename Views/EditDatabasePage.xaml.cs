using AMK.Models.ViewModels;
using AMK.Services;

namespace AMK.Views;

public partial class EditDatabase : ContentPage
{
    UsersViewModel _viewModel;

    public EditDatabase()
	{
		InitializeComponent();
        BindingContext = new UsersViewModel(
            Application.Current.Handler.MauiContext.Services.GetService<IDatabaseService>());
    }
}