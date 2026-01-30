using AMK.Models.ViewModels;

namespace AMK.Views;

[QueryProperty(nameof(NewsId), "newsId")]
public partial class CompanyNewsDetailPage : ContentPage
{
    private readonly CompanyNewsDetailViewModel _vm;
    private int _newsId;

    public string NewsId
    {
        get => _newsId.ToString();
        set
        {
            if (int.TryParse(value, out var id))
                _newsId = id;
        }
    }

    public CompanyNewsDetailPage(CompanyNewsDetailViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_newsId > 0)
            await _vm.LoadAsync(_newsId);
    }
}
