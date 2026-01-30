using AMK.Models.Headlines;
using AMK.Models.Rss;
using AMK.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
namespace AMK.Models.ViewModels
{
    public partial class NewsViewModel : INotifyPropertyChanged 
    {
        

        private readonly INewsService _newsService;


        private ObservableCollection<RssNewsItems> _newsItems = new();
        private bool _isLoading;
        private bool _hasError;
        private string _errorMessage = string.Empty;
        private bool _hasMoreNews = true;
        private int _skip = 0;
        private const int PageSize = 15;


        public ObservableCollection<RssNewsItems> NewsItems
        {
            get => _newsItems;
            set
            {
                if (_newsItems != value)
                {
                    _newsItems = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasError
        {
            get => _hasError;
            set
            {
                if (_hasError != value)
                {
                    _hasError = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasMoreNews
        {
            get => _hasMoreNews;
            set
            {
                if (_hasMoreNews != value)
                {
                    _hasMoreNews = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand => new Command(async () => await RefreshNewsAsync());
        public ICommand LoadMoreCommand => new Command(async () => await LoadMoreAsync());

        public NewsViewModel(INewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task LoadNewsAsync()
        {
            await LoadFirstPageAsync();
        }

        public async Task RefreshNewsAsync()
        {
            await LoadFirstPageAsync();
        }

        private async Task LoadFirstPageAsync()
        {
            if (IsLoading) return;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;
            _skip = 0;

            try
            {
                var news = await _newsService.LoadAllNewsAsync(_skip, PageSize);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NewsItems.Clear();
                    foreach (var item in news)
                        NewsItems.Add(item);

                    HasMoreNews = news.Count == PageSize;
                    _skip = NewsItems.Count;
                });
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMoreAsync()
        {
            if (IsLoading || !HasMoreNews) return;
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var news = await _newsService.LoadAllNewsAsync(_skip, PageSize);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    foreach (var item in news)
                        NewsItems.Add(item);

                    HasMoreNews = news.Count == PageSize;
                    _skip = NewsItems.Count;
                });
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка загрузки: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

