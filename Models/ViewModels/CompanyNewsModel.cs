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
    public partial class CompanyNewsModel : ObservableObject
    {
        ICompanyNews _companyNews;
        SessionService _sessionService;
        [ObservableProperty]
        private ObservableCollection<News> _newsItems = new();

        [ObservableProperty]
        private News _selectedNews;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isRefreshing;

        [ObservableProperty]
        private bool _hasError;

        [ObservableProperty]
        private bool _isEditMode;

        [ObservableProperty]
        private string _errorMessage;

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


        public CompanyNewsModel(ICompanyNews companyNews, SessionService sessionService)
        {
            _companyNews = companyNews;
            _sessionService = sessionService;
            LoadNewsCommand = new AsyncRelayCommand(LoadNewsAsync);
            RefreshNewsCommand = new AsyncRelayCommand(RefreshNewsAsync);
            AddNewsCommand = new RelayCommand(StartAddNews);
            EditNewsCommand = new RelayCommand(StartEditNews);
            SaveNewsCommand = new AsyncRelayCommand(SaveNewsAsync);
            DeleteNewsCommand = new AsyncRelayCommand(DeleteNewsAsync);
            CancelEditCommand = new RelayCommand(CancelEdit);
            SelectNewsCommand = new RelayCommand<News>(SelectNews);
        }

       
        private async Task LoadNewsAsync()
        {
            if (IsLoading) return;

            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            try
            {
                var newsList = await _companyNews.GetAllNewsAsync();

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

        private async Task RefreshNewsAsync()
        {
            IsRefreshing = true;
            await LoadNewsAsync();
        }

        private async Task SaveNewsAsync()
        {
            if (string.IsNullOrWhiteSpace(Title) ||
                string.IsNullOrWhiteSpace(Content) ||
                string.IsNullOrWhiteSpace(ShortDescription))
            {
                ErrorMessage = "Заполните все обязательные поля";
                HasError = true;
                return;
            }

            try
            {
                var currentUser = _sessionService.CurrentUser;
                if (currentUser == null)
                {
                    ErrorMessage = "Пользователь не авторизован";
                    HasError = true;
                    return;
                }

                if (SelectedNews == null)
                {
                    // Добавление новой новости
                    var newNews = new News
                    {
                        Title = Title,
                        Content = Content,
                        ShortDescription = ShortDescription,
                        AuthorID = currentUser.ID_user,
                        PublishDate = PublishDate,
                        CreatedAt = DateTime.Now
                    };

                    var result = await _companyNews.AddNewsAsync(newNews);
                    if (result > 0)
                    {
                        await LoadNewsAsync();
                        CancelEdit();
                    }
                }
                else
                {
                    // Редактирование существующей новости
                    SelectedNews.Title = Title;
                    SelectedNews.Content = Content;
                    SelectedNews.ShortDescription = ShortDescription;
                    SelectedNews.PublishDate = PublishDate;

                    var result = await _companyNews.UpdateNewsAsync(SelectedNews);
                    if (result > 0)
                    {
                        await LoadNewsAsync();
                        CancelEdit();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка сохранения: {ex.Message}";
                HasError = true;
            }
        }

        private async Task DeleteNewsAsync()
        {
            if (SelectedNews == null) return;

            bool confirm = await Application.Current.MainPage.DisplayAlert(
                "Подтверждение",
                $"Вы действительно хотите удалить новость \"{SelectedNews.Title}\"?",
                "Да",
                "Нет");

            if (!confirm) return;

            try
            {
                var result = await _companyNews.DeleteNewsAsync(SelectedNews.ID_news);
                if (result > 0)
                {
                    NewsItems.Remove(SelectedNews);
                    SelectedNews = null;
                    CancelEdit();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Ошибка удаления: {ex.Message}";
                HasError = true;
            }
        }
        private void CancelEdit()
        {
            IsEditMode = false;
            SelectedNews = null;
            ClearForm();
        }

        private void SelectNews(News news)
        {
            SelectedNews = news;
            IsEditMode = false;
        }

        private void ClearForm()
        {
            Title = string.Empty;
            Content = string.Empty;
            ShortDescription = string.Empty;
            PublishDate = DateTime.Now;
        }

        partial void OnSelectedNewsChanged(News value)
        {
            OnPropertyChanged(nameof(IsNewsSelected));
        }

        public bool IsNewsSelected => SelectedNews != null;

        private void StartAddNews()
        {
            IsEditMode = true;
            SelectedNews = null;
            ClearForm();
            PublishDate = DateTime.Now;
        }

        private void StartEditNews()
        {
            if (SelectedNews == null) return;

            IsEditMode = true;
            Title = SelectedNews.Title;
            Content = SelectedNews.Content;
            ShortDescription = SelectedNews.ShortDescription;
            PublishDate = SelectedNews.PublishDate;
        }
    }
}
