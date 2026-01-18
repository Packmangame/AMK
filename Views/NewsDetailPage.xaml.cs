namespace AMK.Views;

public partial class NewsDetailPage : ContentPage
{
    string _newsUrl;
    public NewsDetailPage(string newsUrl)
	{
		InitializeComponent();
        _newsUrl = newsUrl;
       
        NewsWebView.Source = newsUrl;
    }

    private void OnWebViewNavigating(object sender, WebNavigatingEventArgs e)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
    }

    private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
    {
        LoadingIndicator.IsVisible = false;
        LoadingIndicator.IsRunning = false;
    }

}