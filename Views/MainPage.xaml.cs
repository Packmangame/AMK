using AMK.Models.Headlines;
using AMK.Models.Rss;
using AMK.Models.Users;
using AMK.Models.ViewModels;
using AMK.Services;
namespace AMK.Views;

public partial class MainPage : ContentPage
{
    private readonly NewsViewModel _viewModel;

    public MainPage(NewsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.NewsItems.Count == 0 && !_viewModel.IsLoading)
            await _viewModel.LoadNewsAsync();
    }
    private async void OnReadNewsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is RssNewsItems newsItem)
        {
            if (!string.IsNullOrEmpty(newsItem.Link))
            {
                await Navigation.PushAsync(new NewsDetailPage(newsItem.Link));
                newsItem.IsRead = true;
            }
        }
    }

 
}