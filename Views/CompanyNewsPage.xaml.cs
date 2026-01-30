using AMK.Models.ViewModels;

namespace AMK.Views;

public partial class CompanyNewsPage : ContentPage
{
    private readonly CompanyNewsListViewModel _vm;

    public CompanyNewsPage(CompanyNewsListViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
