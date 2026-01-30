using AMK.Models.Users;
using AMK.Services;
using AMK.Validation;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AMK.Models.ViewModels
{
    public partial class UsersViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<User> users = new();

        [ObservableProperty]
        private User selectedUser = new()
        {
            Birthday = DateTime.Now.AddYears(-25)
        };

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool isEditing;

        [ObservableProperty]
        private List<Role> roles = new();

        [ObservableProperty]
        private List<Department> departments = new();

        [ObservableProperty]
        private Role selectedRole;

        [ObservableProperty]
        private Department selectedDepartment;

        public UsersViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var usersList = await _databaseService.GetAllUsersAsync();
                Users = new ObservableCollection<User>(usersList);

                Roles = await _databaseService.GetAllRolesAsync();
                Departments = await _databaseService.GetAllDepartmentsAsync();

                // синхронизируем выбранные значения для пикеров
                SelectedRole = Roles.FirstOrDefault(r => r.ID_role == SelectedUser.Role) ?? Roles.FirstOrDefault();
                SelectedDepartment = Departments.FirstOrDefault(d => d.ID_depart == SelectedUser.Department) ?? Departments.FirstOrDefault();

                if (SelectedUser == null)
                    SelectedUser = new User { Birthday = DateTime.Now.AddYears(-25) };
            }
            catch (Exception ex)
            {
                await Toast.Make($"Не удалось загрузить данные: {ex.Message}").Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            try
            {
                IsLoading = true;
                var results = await _databaseService.SearchUsersAsync(SearchText);
                Users = new ObservableCollection<User>(results);
            }
            catch (Exception ex)
            {
                await Toast.Make($"Ошибка поиска: {ex.Message}").Show();
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NewUser()
        {
            SelectedUser = new User
            {
                Birthday = DateTime.Now.AddYears(-25),
                Role = Roles.FirstOrDefault()?.ID_role ?? 1,
                Department = Departments.FirstOrDefault()?.ID_depart ?? 1
            };
            SelectedRole = Roles.FirstOrDefault(r => r.ID_role == SelectedUser.Role) ?? Roles.FirstOrDefault();
            SelectedDepartment = Departments.FirstOrDefault(d => d.ID_depart == SelectedUser.Department) ?? Departments.FirstOrDefault();
            IsEditing = true;
        }

        [RelayCommand]
        private void EditUser(User user)
        {
            if (user == null) return;

            SelectedUser = new User
            {
                ID_user = user.ID_user,
                FIO = user.FIO,
                Birthday = user.Birthday,
                Department = user.Department,
                Phone = user.Phone,
                Role = user.Role,
                Login = user.Login,
                Password = user.Password,
                Email = user.Email,
                Tests = user.Tests
            };

            SelectedRole = Roles.FirstOrDefault(r => r.ID_role == SelectedUser.Role) ?? Roles.FirstOrDefault();
            SelectedDepartment = Departments.FirstOrDefault(d => d.ID_depart == SelectedUser.Department) ?? Departments.FirstOrDefault();

            IsEditing = true;
        }

        [RelayCommand]
        private void CancelEdit()
        {
            IsEditing = false;
            SelectedUser = new User { Birthday = DateTime.Now.AddYears(-25) };
            SelectedRole = Roles.FirstOrDefault();
            SelectedDepartment = Departments.FirstOrDefault();
        }

        partial void OnSelectedRoleChanged(Role value)
        {
            if (value != null && SelectedUser != null)
                SelectedUser.Role = value.ID_role;
        }

        partial void OnSelectedDepartmentChanged(Department value)
        {
            if (value != null && SelectedUser != null)
                SelectedUser.Department = value.ID_depart;
        }

      

        [RelayCommand]
        private async Task SaveUserAsync()
        {
            // Валидации
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SelectedUser?.FIO))
                errors.Add("ФИО - обязательное поле");

            if (string.IsNullOrWhiteSpace(SelectedUser?.Login) || !Validators.IsValidLogin(SelectedUser.Login))
                errors.Add("Логин - обязательное поле (3-32 символа)");

            if (string.IsNullOrWhiteSpace(SelectedUser?.Password) || !Validators.IsValidPassword(SelectedUser.Password))
                errors.Add("Пароль: минимум 8 символов, 1 заглавная, 1 спецсимвол");
          
           if (!Validators.IsValidEmail(SelectedUser.Email))
                errors.Add("Некорректный Email (пример: name@domain.ru)");

            if (SelectedUser != null && !Validators.IsValidBirthDate(SelectedUser.Birthday))
                errors.Add("Дата рождения: возраст от 16 до 120 лет");

            if (errors.Count > 0)
            {
                await Shell.Current.DisplayAlert("Проверьте поля", string.Join("\n", errors), "OK");
                return;
            }

            try
            {
                IsLoading = true;

                if (SelectedUser.ID_user == 0)
                {
                    var created = await _databaseService.CreateUserAsync(SelectedUser);
                    if (created > 0)
                        await LoadDataAsync();
                }
                else
                {
                    var updated = await _databaseService.UpdateUserAsync(SelectedUser);
                    if (updated > 0)
                        await LoadDataAsync();
                }

                CancelEdit();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка сохранения: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DeleteUserAsync(User user)
        {
            if (user == null) return;

            bool confirm = await Shell.Current.DisplayAlert(
                "Подтверждение",
                $"Вы действительно хотите удалить пользователя {user.FIO}?",
                "Да",
                "Нет");

            if (!confirm) return;

            try
            {
                IsLoading = true;
                var result = await _databaseService.DeleteUserAsync(user.ID_user);
                if (result > 0)
                    Users.Remove(user);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка удаления: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        
    }
}
