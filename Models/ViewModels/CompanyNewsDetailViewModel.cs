using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AMK.Models.Headlines;
using AMK.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace AMK.Models.ViewModels;

public partial class CompanyNewsDetailViewModel : ObservableObject
{
    private readonly ICompanyNews _companyNews;

    [ObservableProperty] private News? item;
    public ObservableCollection<NewsMedia> Media { get; } = new();

    public CompanyNewsDetailViewModel(ICompanyNews companyNews)
    {
        _companyNews = companyNews;
    }

    [RelayCommand]
    public async Task LoadAsync(int newsId)
    {
        var news = await _companyNews.GetNewsByIdAsync(newsId);
        Item = news;
        var media = await _companyNews.GetMediaForNewsAsync(newsId);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Media.Clear();
            foreach (var m in media) Media.Add(m);
        });
    }

    [RelayCommand]
    public async Task OpenMediaAsync(NewsMedia? media)
    {
        if (media == null) return;
        // For images we show inline in UI; for others open via system.
        var ext = Path.GetExtension(media.MediaUrl).ToLowerInvariant();
        if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp")
            return;
        try
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(media.MediaUrl)
            });
        }
        catch
        {
            // ignore
        }
    }
}
