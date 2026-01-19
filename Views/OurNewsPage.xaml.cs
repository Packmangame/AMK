using AMK.Models.ViewModels;
using AMK.Services;

namespace AMK.Views;

public partial class OurNewsPage : ContentPage
{
	Models.Users.User _user;
    CompanyNewsModel _viewModel;
    public OurNewsPage()
	{
		InitializeComponent();
		_user = SessionService.Instance.CurrentUser;
        _viewModel = new CompanyNewsModel();
        BindingContext = _viewModel;
        
        if (_user.Role == 1)
        {
            var createButton = new Button
            {
                Text = "Создать новость",
            };
            createButton.Clicked += async (sender, e) =>
            {
                await Shell.Current.GoToAsync($"//{nameof(EditCompanyNewsPage)}");
            };

            if (Content is Grid grid)
            {
                grid.RowDefinitions.Insert(0, new RowDefinition { Height = GridLength.Auto });

                for (int i = 0; i < grid.Children.Count; i++)
                {
                    if (grid.Children[i] is View view)
                    {
                        Grid.SetRow(view, Grid.GetRow(view) + 1);
                    }
                }

                Grid.SetRow(createButton, 0);
                Grid.SetColumnSpan(createButton, 4);
                grid.Children.Add(createButton);
            }
           
        }
        
    }
}