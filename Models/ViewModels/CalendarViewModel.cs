using AMK.Models.Calendar;
using AMK.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AMK.Models.ViewModels
{
    public partial class CalendarViewModel : ObservableObject
    {
        private readonly IDatabaseService _db;

        [ObservableProperty]
        private ObservableCollection<CalendarEvent> upcomingEvents = new();

        [ObservableProperty]
        private bool isLoading;

        public CalendarViewModel(IDatabaseService db)
        {
            _db = db;
            _ = LoadAsync();
        }

        [RelayCommand]
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                var events = new List<CalendarEvent>();

                // Базовые корпоративные даты (пример - можно расширять)
                var year = DateTime.Now.Year;
                events.Add(new CalendarEvent { Date = new DateTime(year, 1, 1), Description = "Новый год", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 1, 7), Description = "Рождество", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 2, 23), Description = "23 февраля", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 3, 8), Description = "8 марта", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 5, 1), Description = "1 мая", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 5, 9), Description = "9 мая", Type = "Праздник" });
                events.Add(new CalendarEvent { Date = new DateTime(year, 6, 12), Description = "День России", Type = "Праздник" });

                // Дни рождения сотрудников (из БД)
                var users = await _db.GetAllUsersAsync();
                foreach (var u in users)
                {
                    if (u?.Birthday == default) continue;
                    var next = new DateTime(DateTime.Now.Year, u.Birthday.Month, u.Birthday.Day);
                    if (next < DateTime.Today) next = next.AddYears(1);
                    events.Add(new CalendarEvent { Date = next, Description = $"День рождения: {u.FIO}", Type = "Сотрудник" });
                }

                var upcoming = events
                    .Where(e => e.Date >= DateTime.Today)
                    .OrderBy(e => e.Date)
                    .Take(30)
                    .ToList();

                UpcomingEvents = new ObservableCollection<CalendarEvent>(upcoming);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NotifyAsync()
        {
            await Shell.Current.DisplayAlert("Уведомления", "Уведомления о событиях будут добавлены в следующей итерации.", "ОК");
        }
    }
}
