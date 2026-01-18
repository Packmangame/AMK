using AMK.Models.Headlines;
using AMK.Models.Users;
using CommunityToolkit.Maui.Alerts;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Services
{
    public class DatabaseService : IDatabaseService
    {
        private bool _isInitialized = false;
        private readonly SQLiteAsyncConnection _database;
        public DatabaseService()
        {
            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "AMK.db3");
            _database = new SQLiteAsyncConnection(databasePath);

            // Запускаем инициализацию в фоне, не блокируя UI
            Task.Run(async () => await InitializeAsync());
        }


        private async Task InitializeAsync()
        {
            try
            {
                if (_isInitialized) return;

                await _database.CreateTableAsync<User>();
                await _database.CreateTableAsync<Role>();
                await _database.CreateTableAsync<Department>();
                await _database.CreateTableAsync<News>();
                await _database.CreateTableAsync<NewsMedia>();
                await _database.CreateTableAsync<MediaTypes>();

                await SeedInitialDataAsync();
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Initialize error: {ex.Message}");
            }
        }
        private async Task SeedInitialDataAsync()
        {
            try
            {
                if (await _database.Table<Role>().CountAsync() == 0)
                {
                    var roles = new List<Role>
                    {
                        new Role { Title = "Admin" },
                        new Role { Title = "User" }
                       
                    };

                    await _database.InsertAllAsync(roles);
                    Debug.WriteLine($"✅ Добавлено {roles.Count} ролей");
                }

                // Отделы
                if (await _database.Table<Department>().CountAsync() == 0)
                {
                    var departments = new List<Department>
                    {
                        new Department { DepartmentName = "Администрация" },
                        new Department { DepartmentName = "IT-отдел" },
                        new Department { DepartmentName = "Отдел маркетигра" },
                        new Department { DepartmentName = "Бухгалтерия" }
                    };

                    await _database.InsertAllAsync(departments);
                    Debug.WriteLine($"✅ Добавлено {departments.Count} отделов");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"⚠️ Ошибка добавления начальных данных: {ex.Message}");
            }
        }

        public async Task<User> GetUserByIdAsync(int userId)
        {
            try
            {
                // Асинхронный поиск по первичному ключу
                var user = await _database.Table<User>()
                    .Where(u => u.ID_user == userId)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    // Загружаем связанные данные (роль и отдел)
                    user.RoleDetail = await GetRoleByIdAsync(user.Role);
                    user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
                }

                return user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка получения пользователя: {ex.Message}");
                return null;
            }
        }
        public async Task<int> CreateUserAsync(User user)
        {
            try
            {
                var result = await _database.InsertAsync(user);

                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка создания пользователя: {ex.Message}");
                return 0;
            }
        }
        private async Task<User> GetUserByLoginAsync(string login)
        {
            try
            {
                return await _database.Table<User>()
                    .Where(u => u.Login == login)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка получения всех пользователей: {ex.Message}");
                return null;
            }
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            try
            {
                // Асинхронно получаем всех пользователей с сортировкой
                var users = await _database.Table<User>()
                    .OrderBy(u => u.FIO)
                    .ToListAsync();

                // Загружаем связанные данные для каждого пользователя
                foreach (var user in users)
                {
                    user.RoleDetail = await GetRoleByIdAsync(user.Role);
                    user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка получения всех пользователей: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<int> UpdateUserAsync(User user)
        {
            try
            {
                // Обновляем запись в БД
                var result = await _database.UpdateAsync(user);
                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка обновления пользователя: {ex.Message}");
                return 0;
            }
          
        }

        public async Task<int> DeleteUserAsync(int userId)
        {
            try
            {
                var result = await _database.DeleteAsync<User>(userId);
                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка удаления пользователя: {ex.Message}");
                return 0;
            }
           
        }

        public async Task<List<User>> SearchUsersAsync(string keyword)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(keyword))
                    return await GetAllUsersAsync();

                // Асинхронный поиск по нескольким полям
                var users = await _database.Table<User>()
                    .Where(u => u.FIO.Contains(keyword) ||
                               u.Login.Contains(keyword) ||
                               u.Email.Contains(keyword) ||
                               u.Phone.Contains(keyword))
                    .OrderBy(u => u.FIO)
                    .ToListAsync();

                // Загружаем связанные данные
                foreach (var user in users)
                {
                    user.RoleDetail = await GetRoleByIdAsync(user.Role);
                    user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка поиска пользователей: {ex.Message}");
                return new List<User>();
            }
        }

        public async Task<int> CreateRoleAsync(Role role)
        {
            var result = await _database.InsertAsync(role);
            await Toast.Make($"Успешно").Show();
            return result;
        }
        public async Task<Role> GetRoleByIdAsync(int roleId)
        {
            try
            {
                return await _database.Table<Role>()
                    .Where(r => r.ID_role == roleId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private async Task<Role> GetRoleByTitleAsync(string title)
        {
            try
            {
                return await _database.Table<Role>()
                    .Where(r => r.Title == title)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<Role>> GetAllRolesAsync()
        {
            try
            {
                return await _database.Table<Role>()
                    .OrderBy(r => r.Title)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Role>();
            }
        }
        public async Task<int> UpdateRoleAsync(Role role)
        {
            var result = await _database.UpdateAsync(role);
            await Toast.Make($"Успешно").Show();
            return result;
        }

        public async Task<int> DeleteRoleAsync(int roleId)
        {
            
            var usersCount = await _database.Table<User>()
                .Where(u => u.Role == roleId)
                .CountAsync();

            if (usersCount > 0)
              await Toast.Make($"Нельзя удалить роль, так как {usersCount} пользователей ее используют").Show();

            return await _database.DeleteAsync<Role>(roleId);
        }

        public async Task<int> CreateDepartmentAsync(Department department)
        {
            try
            {
                var result = await _database.InsertAsync(department);
                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private async Task<Department> GetDepartmentByNameAsync(string name)
        {
            try
            {
                return await _database.Table<Department>()
                    .Where(d => d.DepartmentName == name)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<Department> GetDepartmentByIdAsync(int departmentId)
        {
            try
            {
                return await _database.Table<Department>()
                    .Where(d => d.ID_depart == departmentId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<Department>> GetAllDepartmentsAsync()
        {
            try
            {
                return await _database.Table<Department>()
                    .OrderBy(d => d.DepartmentName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                return new List<Department>();
            }
        }

        public async Task<int> DeleteDepartmentAsync(int departmentId)
        {
            try
            {
                var result = await _database.DeleteAsync<Department>(departmentId);

                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<int> UpdateDepartmentAsync(Department department)
        {
            try
            {
                var result = await _database.UpdateAsync(department);
                await Toast.Make($"Успешно").Show();
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<User> AuthenticateAsync(string login, string password)
        {
            try
            {
                // Находим пользователя по логину
                var user = await GetUserByLoginAsync(login);
                if (user != null)
                {
                    // Загружаем связанные данные
                    user.RoleDetail = await GetRoleByIdAsync(user.Role);
                    user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
                    await Toast.Make($"Успешно").Show();
                    return user;
                }
                else
                {
                    await Toast.Make("Пользователь не найден").Show();
                    return null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Ошибка аутентификации: {ex.Message}");
                return null;
            }
        }
    }
}
    

