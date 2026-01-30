using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AMK.Models.Headlines;
using AMK.Services;
using AMK.Views;

namespace AMK.Models.ViewModels;

public partial class CompanyNewsListViewModel : ObservableObject
{
    private readonly ICompanyNews _companyNews;
    private readonly SessionService _session;

    public ObservableCollection<News> Items { get; } = new();

    [ObservableProperty] private bool isLoading;

    public bool IsAdmin => _session.CurrentUser?.Role == 1;

    public CompanyNewsListViewModel(ICompanyNews companyNews, SessionService session)
    {
        _companyNews = companyNews;
        _session = session;
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
                foreach (var n in list)
                    Items.Add(n);
                OnPropertyChanged(nameof(IsAdmin));
            });
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task OpenDetailAsync(int newsId)
    {
        await Shell.Current.GoToAsync($"{nameof(CompanyNewsDetailPage)}?newsId={newsId}");
    }

    [RelayCommand]
    private async Task OpenAdminAsync()
    {
        if (!IsAdmin)
            return;
        await Shell.Current.GoToAsync("//EditCompNews");
    }
}
