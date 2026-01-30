using AMK.Models.ViewModels;

namespace AMK.Views;

public partial class EditCompanyNewsPage : ContentPage
{
    private readonly EditCompanyNewsViewModel _vm;

    public EditCompanyNewsPage(EditCompanyNewsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_vm.IsAdmin)
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make("Доступно только Admin").Show();
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _vm.LoadAsync();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Editor.IsVisible = true;
    }
}