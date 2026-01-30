using AMK.Models.Users;
using AMK.Services;
using AMK.Validation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMK.Models.ViewModels
{
    public partial class EditProfileViewModel : ObservableObject
    {
        private readonly SessionService _session;
        private readonly IDatabaseService _db;

        [ObservableProperty]
        private User editableUser;

        [ObservableProperty]
        private bool isSaving;

        public EditProfileViewModel(SessionService session, IDatabaseService db)
        {
            _session = session;
            _db = db;
            LoadFromSession();
        }

        [RelayCommand]
        private void LoadFromSession()
        {
            var u = _session.CurrentUser;
            if (u == null)
            {
                EditableUser = new User();
                return;
            }

            // Копия - редактировать можно только себя
            EditableUser = new User
            {
                ID_user = u.ID_user,
                FIO = u.FIO,
                Birthday = u.Birthday,
                Department = u.Department,
                Phone = u.Phone,
                Role = u.Role,
                Login = u.Login,
                Password = u.Password,
                Email = u.Email,
                Tests = u.Tests
            };
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (EditableUser == null)
                return;

            var current = _session.CurrentUser;
            if (current == null || current.ID_user != EditableUser.ID_user)
            {
                await Shell.Current.DisplayAlert("Ошибка", "Можно редактировать только свой профиль", "ОК");
                return;
            }

            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(EditableUser.FIO))
                errors.Add("ФИО - обязательное поле");

            if (string.IsNullOrWhiteSpace(EditableUser.Email))
                errors.Add("Email - обязательное поле");
            else if (!Validators.IsValidEmail(EditableUser.Email))
                errors.Add("Некорректный Email (пример: name@domain.ru)");

            if (string.IsNullOrWhiteSpace(EditableUser.Phone))
                errors.Add("Телефон - обязательное поле");
            if (!Validators.IsValidBirthDate(EditableUser.Birthday))
                errors.Add("Дата рождения: возраст от 16 до 120 лет");

            if (errors.Count > 0)
            {
                await Shell.Current.DisplayAlert("Проверьте поля", string.Join("\n", errors), "OK");
                return;
            }

            try
            {
                IsSaving = true;
                await _db.UpdateUserAsync(EditableUser);

                // Обновляем сессию из базы (с загрузкой Role/Department)
                var updated = await _db.GetUserByIdAsync(EditableUser.ID_user);
                _session.SetCurrentUser(updated);

                await Shell.Current.DisplayAlert("Готово", "Профиль обновлён", "ОК");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", ex.Message, "OK");
            }
            finally
            {
                IsSaving = false;
            }
        }
    }
}
