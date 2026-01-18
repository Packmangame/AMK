using AMK.Models.Headlines;
using AMK.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AMK.Models.ViewModels
{
    public partial class CompanyNewsModel: ObservableObject
    {
        private readonly ICompanyNews _newsRepository;
        private readonly SessionService _sessionService;

        [ObservableProperty]
        private ObservableCollection<News> _newsItems = new();

        [ObservableProperty]
        private News _selectedNews;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private string _errorMessage;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _content;

        [ObservableProperty]
        private string _shortDescription;

        [ObservableProperty]
        private DateTime _publishDate = DateTime.Now;

        public ICommand LoadNewsCommand { get; }
        public ICommand RefreshNewsCommand { get; }
        public ICommand AddNewsCommand { get; }
        public ICommand EditNewsCommand { get; }
        public ICommand SaveNewsCommand { get; }
        public ICommand DeleteNewsCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SelectNewsCommand { get; }

        //public NewsViewModel(ICompanyNews newsRepository, SessionService sessionService)
        //{
        //    _newsRepository = newsRepository;
        //    _sessionService = sessionService;

        //    LoadNewsCommand = new AsyncRelayCommand(LoadNewsAsync);
        //    //RefreshNewsCommand = new AsyncRelayCommand(RefreshNewsAsync);
        //    //AddNewsCommand = new RelayCommand(StartAddNews);
        //    //EditNewsCommand = new RelayCommand(StartEditNews);
        //    //SaveNewsCommand = new AsyncRelayCommand(SaveNewsAsync);
        //    //DeleteNewsCommand = new AsyncRelayCommand(DeleteNewsAsync);
        //    //CancelEditCommand = new RelayCommand(CancelEdit);
        //    //SelectNewsCommand = new RelayCommand<News>(SelectNews);
        //}

        private async Task LoadNewsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var newsList = await _newsRepository.GetAllNewsAsync();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    NewsItems.Clear();
                    foreach (var news in newsList)
                    {
                        NewsItems.Add(news);
                    }
                });
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"Ошибка загрузки новостей: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                IsRefreshing = false;
            }
        }
    }
}
