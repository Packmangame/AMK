using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AMK.Models.Headlines;
using AMK.Services;
using Microsoft.Maui.Storage;

namespace AMK.Models.ViewModels;

public partial class EditCompanyNewsViewModel : ObservableObject
{
    private readonly ICompanyNews _companyNews;
    private readonly SessionService _session;

    public ObservableCollection<News> Items { get; } = new();
    public ObservableCollection<NewsMedia> Media { get; } = new();

    [ObservableProperty] private News? selected;
    [ObservableProperty] private bool isLoading;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string shortDescription = string.Empty;
    [ObservableProperty] private string content = string.Empty;
    [ObservableProperty] private DateTime publishDate = DateTime.Today;

    public bool IsAdmin => _session.CurrentUser?.Role == 1;
    public bool IsNewsSelected => Selected != null;

    public EditCompanyNewsViewModel(ICompanyNews companyNews, SessionService session)
    {
        _companyNews = companyNews;
        _session = session;
    }

    partial void OnSelectedChanged(News? value)
    {
        OnPropertyChanged(nameof(IsNewsSelected));
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        if (IsLoading) return;
        IsLoading = true;
        try
        {
            var list = await _companyNews.GetAllNewsAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Items.Clear();
                foreach (var n in list) Items.Add(n);
                OnPropertyChanged(nameof(IsAdmin));
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public void StartAdd()
    {
        if (!IsAdmin) return;
        Selected = null;
        Title = string.Empty;
        ShortDescription = string.Empty;
        Content = string.Empty;
        PublishDate = DateTime.Today;
        Media.Clear();
    }

    [RelayCommand]
    public async Task StartEditAsync(News? news)
    {
        if (!IsAdmin || news == null) return;
        Selected = news;
        Title = news.Title;
        ShortDescription = news.ShortDescription;
        Content = news.Content;
        PublishDate = news.PublishDate == default ? DateTime.Today : news.PublishDate;
        await LoadMediaAsync(news.ID_news);
    }

    private async Task LoadMediaAsync(int newsId)
    {
        var list = await _companyNews.GetMediaForNewsAsync(newsId);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Media.Clear();
            foreach (var m in list) Media.Add(m);
        });
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        if (!IsAdmin) return;
        if (string.IsNullOrWhiteSpace(Title) || string.IsNullOrWhiteSpace(ShortDescription) || string.IsNullOrWhiteSpace(Content))
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make("Заполните заголовок, краткое описание и текст").Show();
            return;
        }

        var user = _session.CurrentUser;
        if (user == null)
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make("Пользователь не авторизован").Show();
            return;
        }

        if (Selected == null)
        {
            var news = new News
            {
                Title = Title.Trim(),
                ShortDescription = ShortDescription.Trim(),
                Content = Content.Trim(),
                AuthorID = user.ID_user,
                PublishDate = PublishDate,
                CreatedAt = DateTime.Now
            };
            await _companyNews.AddNewsAsync(news);
        }
        else
        {
            Selected.Title = Title.Trim();
            Selected.ShortDescription = ShortDescription.Trim();
            Selected.Content = Content.Trim();
            Selected.PublishDate = PublishDate;
            await _companyNews.UpdateNewsAsync(Selected);
        }

        await LoadAsync();
        StartAdd();
        await CommunityToolkit.Maui.Alerts.Toast.Make("Сохранено").Show();
    }

    [RelayCommand]
    public async Task DeleteAsync(News? news)
    {
        if (!IsAdmin || news == null) return;

        bool confirm = await Application.Current.MainPage.DisplayAlert(
            "Подтверждение",
            $"Удалить новость \"{news.Title}\"?",
            "Удалить",
            "Отмена");
        if (!confirm) return;

        await _companyNews.DeleteNewsAsync(news.ID_news);
        await LoadAsync();
        if (Selected?.ID_news == news.ID_news) StartAdd();
        await CommunityToolkit.Maui.Alerts.Toast.Make("Удалено").Show();
    }

    [RelayCommand]
    public async Task AddMediaAsync()
    {
        if (!IsAdmin || Selected == null) 
        {
            await CommunityToolkit.Maui.Alerts.Toast.Make("Сначала выберите новость и сохраните её").Show();
            return;
        }

        var file = await FilePicker.PickAsync();
        if (file == null) return;

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        int typeId = ext switch
        {
            ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp" => 1,
            ".mp4" or ".mov" or ".m4v" => 2,
            ".mp3" or ".wav" or ".m4a" or ".aac" => 3,
            _ => 4
        };

        var targetDir = Path.Combine(FileSystem.AppDataDirectory, "media");
        Directory.CreateDirectory(targetDir);
        var targetPath = Path.Combine(targetDir, $"{Guid.NewGuid():N}{ext}");
        using (var src = await file.OpenReadAsync())
        using (var dst = File.Create(targetPath))
            await src.CopyToAsync(dst);

        var media = new NewsMedia
        {
            ID_news = Selected.ID_news,
            MediaUrl = targetPath,
            IconUrl = "news.png",
            ID_medType = typeId,
            Caption = file.FileName
        };
        await _companyNews.AddMediaAsync(media);
        await LoadMediaAsync(Selected.ID_news);
        await CommunityToolkit.Maui.Alerts.Toast.Make("Файл добавлен").Show();
    }

    [RelayCommand]
    public async Task DeleteMediaAsync(NewsMedia? media)
    {
        if (!IsAdmin || media == null) return;
        await _companyNews.DeleteMediaAsync(media.ID_newsMedia);
        if (Selected != null)
            await LoadMediaAsync(Selected.ID_news);
    }
}
