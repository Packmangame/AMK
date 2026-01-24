using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMK.Models.Users;
using AMK.Models.Headlines;

namespace AMK.Services
{
    public interface IDatabaseService
    { 
        #region User CRUD
        Task<int> CreateUserAsync(User user); //Администратор
        Task<User> GetUserByIdAsync(int userId); //Вход
        Task<List<User>> GetAllUsersAsync(); //Администратор
        Task<int> UpdateUserAsync(User user); //Администратор
        Task<int> DeleteUserAsync(int userId); //Администратор
        Task<List<User>> SearchUsersAsync(string keyword); //Администратор
        Task<User> AuthenticateAsync(string login, string password); //Вход
        #endregion

        #region Role CRUD
        Task<int> CreateRoleAsync(Role role); //Администратор
        Task<Role> GetRoleByIdAsync(int roleId); //Редактирование Ролей Администратор
        Task<List<Role>> GetAllRolesAsync(); 
        Task<int> UpdateRoleAsync(Role role); //Администратор
        Task<int> DeleteRoleAsync(int roleId); //Администратор
        #endregion

        #region Department CRUD
        Task<int> CreateDepartmentAsync(Department department);
        Task<Department> GetDepartmentByIdAsync(int departmentId);
        Task<List<Department>> GetAllDepartmentsAsync();
        Task<int> UpdateDepartmentAsync(Department department);
        Task<int> DeleteDepartmentAsync(int departmentId);
        #endregion

    }
}
