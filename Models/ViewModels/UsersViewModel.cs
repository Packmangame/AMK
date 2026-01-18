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
    }
}
