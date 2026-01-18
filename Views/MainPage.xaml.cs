using AMK.Models.Headlines;
using AMK.Models.Rss;
using AMK.Models.Users;
using AMK.Models.ViewModels;
using AMK.Services;
namespace AMK.Views;

public partial class MainPage : ContentPage
{
    NewsViewModel _viewModel;
    RssNewsItems _currentArticle;
    User _user;
    public MainPage()
    {
        InitializeComponent();
        var newsService = new RssNewsServices();
        _viewModel = new NewsViewModel(newsService);
        BindingContext = _viewModel;
        LoadNews();
    }
    private async void LoadNews()
    {
        if (_viewModel != null && !_viewModel.IsLoading)
        {
            await _viewModel.LoadNewsAsync();
        }
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

    private void OnLoadMoreClicked(object sender, EventArgs e)
    {
        LoadNews();
    }

   
}