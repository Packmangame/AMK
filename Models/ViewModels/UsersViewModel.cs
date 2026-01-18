using AMK.Models.Users;
using AMK.Services;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.ViewModels
{
    public partial class UsersViewModel: ObservableObject
    {
        private readonly IDatabaseService _databaseService;

        [ObservableProperty]
        private ObservableCollection<User> _users;

        [ObservableProperty]
        private User _selectedUser;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEditing;

        [ObservableProperty]
        private List<Role> _roles;

        [ObservableProperty]
        private List<Department> _departments;

        public UsersViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
            Users = new ObservableCollection<User>();
            SelectedUser = new User
            {
                Birthday = DateTime.Now.AddYears(-25)
            };

            LoadData();
        }

        [RelayCommand]
        private async Task LoadData()
        {
            try
            {
                IsLoading = true;

                // Загружаем пользователей
                var users = await _databaseService.GetAllUsersAsync();
                Users.Clear();
                foreach (var user in users)
                {
                    Users.Add(user);
                }

                // Загружаем роли и отделы для формы
                Roles = await _databaseService.GetAllRolesAsync();
                Departments = await _databaseService.GetAllDepartmentsAsync();
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
        private async Task Search()
        {
            try
            {
                IsLoading = true;
                var searchResults = await _databaseService.SearchUsersAsync(SearchText);

                Users.Clear();
                foreach (var user in searchResults)
                {
                    Users.Add(user);
                }
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
                Role = Roles?.FirstOrDefault()?.ID_role ?? 1,
                Department = Departments?.FirstOrDefault()?.ID_depart ?? 1
            };
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
            IsEditing = true;
        }

        [RelayCommand]
        private async Task SaveUser()
        {
            if (string.IsNullOrWhiteSpace(SelectedUser.FIO) ||
                string.IsNullOrWhiteSpace(SelectedUser.Login) ||
                string.IsNullOrWhiteSpace(SelectedUser.Password))
            {
                await Toast.Make("Заполните обязательные поля").Show();
                return;
            }

            try
            {
                IsLoading = true;

                int result;
                if (SelectedUser.ID_user == 0)
                {
                    result = await _databaseService.CreateUserAsync(SelectedUser);
                    if (result > 0)
                    {
                        await LoadData();
                    }
                }
                else
                {
                    result = await _databaseService.UpdateUserAsync(SelectedUser);
                    if (result > 0)
                    {
                        await LoadData();
                    }
                }

                IsEditing = false;
                SelectedUser = new User { Birthday = DateTime.Now.AddYears(-25) };
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Ошибка сохранения: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }

            [RelayCommand]
             async Task DeleteUser(User user)
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
                    {
                        Users.Remove(user);
                    }
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

            [RelayCommand]
             async Task Refresh()
            {
                await LoadData();
            }
        }
    }
}
