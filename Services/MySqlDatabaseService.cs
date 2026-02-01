using System.Diagnostics;
using AMK.Config;
using AMK.Models.Users;
using CommunityToolkit.Maui.Alerts;
using MySqlConnector;

namespace AMK.Services;

/// <summary>
/// Реализация IDatabaseService поверх MySQL (Timeweb Cloud).
/// Работает СТРОГО с таблицами и полями как в вашей БД:
/// - Roles(ID_role, Title)
/// - Departments(ID_depart, DepartmentName)
/// - Users(ID_user, FIO, Login, Password, Email, Phone, Birthday, Role, DepartmentId)
/// </summary>
public sealed class MySqlDatabaseService : IDatabaseService
{
    private readonly string _cs;

    public MySqlDatabaseService()
    {
        _cs = DbConfig.MySqlConnectionString;
    }

    private async Task<MySqlConnection> OpenAsync()
    {
        var conn = new MySqlConnection(_cs);
        await conn.OpenAsync();
        return conn;
    }

    #region Users

    public async Task<int> CreateUserAsync(User user)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
INSERT INTO `Users` (FIO, Login, Password, Email, Phone, Birthday, Role, DepartmentId)
VALUES (@fio, @login, @pass, @email, @phone, @birthday, @role, @departmentId);
SELECT LAST_INSERT_ID();";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fio", user.FIO);
            cmd.Parameters.AddWithValue("@login", user.Login);
            cmd.Parameters.AddWithValue("@pass", user.Password);
            cmd.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@birthday", user.Birthday.Date);
            cmd.Parameters.AddWithValue("@role", user.Role);
            cmd.Parameters.AddWithValue("@departmentId", user.Department);

            user.ID_user = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await Toast.Make("Успешно").Show();
            return user.ID_user;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateUser error: {ex.Message}");
            return 0;
        }
    }

    public async Task<User> GetUserByIdAsync(int userId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_user,
  FIO,
  Login,
  Password,
  Email,
  Phone,
  Birthday,
  Role,
  DepartmentId AS Department
FROM `Users`
WHERE ID_user=@id
LIMIT 1;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;

            var user = MapUser(r);
            user.RoleDetail = await GetRoleByIdAsync(user.Role);
            user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
            return user;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetUserById error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        var list = new List<User>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_user,
  FIO,
  Login,
  Password,
  Email,
  Phone,
  Birthday,
  Role,
  DepartmentId AS Department
FROM `Users`
ORDER BY FIO;";

            await using var cmd = new MySqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapUser(r));

            foreach (var u in list)
            {
                u.RoleDetail = await GetRoleByIdAsync(u.Role);
                u.DepartmentDetail = await GetDepartmentByIdAsync(u.Department);
            }

            return list;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllUsers error: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<int> UpdateUserAsync(User user)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
UPDATE `Users` SET
  FIO=@fio,
  Login=@login,
  Password=@pass,
  Email=@email,
  Phone=@phone,
  Birthday=@birthday,
  Role=@role,
  DepartmentId=@departmentId
WHERE ID_user=@id;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@fio", user.FIO);
            cmd.Parameters.AddWithValue("@login", user.Login);
            cmd.Parameters.AddWithValue("@pass", user.Password);
            cmd.Parameters.AddWithValue("@email", (object?)user.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone", (object?)user.Phone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@birthday", user.Birthday.Date);
            cmd.Parameters.AddWithValue("@role", user.Role);
            cmd.Parameters.AddWithValue("@departmentId", user.Department);
            cmd.Parameters.AddWithValue("@id", user.ID_user);

            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateUser error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteUserAsync(int userId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "DELETE FROM `Users` WHERE ID_user=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteUser error: {ex.Message}");
            return 0;
        }
    }

    public async Task<List<User>> SearchUsersAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return await GetAllUsersAsync();

        var list = new List<User>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_user,
  FIO,
  Login,
  Password,
  Email,
  Phone,
  Birthday,
  Role,
  DepartmentId AS Department
FROM `Users`
WHERE FIO LIKE @k OR Login LIKE @k OR Email LIKE @k OR Phone LIKE @k
ORDER BY FIO;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@k", $"%{keyword}%");
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(MapUser(r));

            foreach (var u in list)
            {
                u.RoleDetail = await GetRoleByIdAsync(u.Role);
                u.DepartmentDetail = await GetDepartmentByIdAsync(u.Department);
            }

            return list;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"SearchUsers error: {ex.Message}");
            return new List<User>();
        }
    }

    public async Task<User> AuthenticateAsync(string login, string password)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = @"
SELECT
  ID_user,
  FIO,
  Login,
  Password,
  Email,
  Phone,
  Birthday,
  Role,
  DepartmentId AS Department
FROM `Users`
WHERE Login=@login
LIMIT 1;";

            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@login", login);

            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
            {
                await Toast.Make("Пользователь не найден").Show();
                return null;
            }

            var user = MapUser(r);
            if (!string.Equals(user.Password, password, StringComparison.Ordinal))
            {
                await Toast.Make("Неверный пароль").Show();
                return null;
            }

            user.RoleDetail = await GetRoleByIdAsync(user.Role);
            user.DepartmentDetail = await GetDepartmentByIdAsync(user.Department);
            await Toast.Make("Успешно").Show();
            return user;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Authenticate error: {ex.Message}");
            return null;
        }
    }

    private static User MapUser(MySqlDataReader r)
    {
        return new User
        {
            ID_user = r.GetInt32("ID_user"),
            FIO = r.GetString("FIO"),
            Login = r.GetString("Login"),
            Password = r.GetString("Password"),
            Email = r.IsDBNull(r.GetOrdinal("Email")) ? null : r.GetString("Email"),
            Phone = r.IsDBNull(r.GetOrdinal("Phone")) ? null : r.GetString("Phone"),
            Birthday = r.GetDateTime("Birthday"),
            Role = r.GetInt32("Role"),
            Department = r.GetInt32("Department"),
            Tests = null
        };
    }

    #endregion

    #region Roles

    public async Task<int> CreateRoleAsync(Role role)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "INSERT INTO `Roles`(Title) VALUES(@t); SELECT LAST_INSERT_ID();";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", role.Title);
            role.ID_role = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await Toast.Make("Успешно").Show();
            return role.ID_role;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateRole error: {ex.Message}");
            return 0;
        }
    }

    public async Task<Role> GetRoleByIdAsync(int roleId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "SELECT ID_role, Title FROM `Roles` WHERE ID_role=@id LIMIT 1;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", roleId);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            return new Role { ID_role = r.GetInt32("ID_role"), Title = r.GetString("Title") };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetRoleById error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Role>> GetAllRolesAsync()
    {
        var list = new List<Role>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "SELECT ID_role, Title FROM `Roles` ORDER BY Title;";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new Role { ID_role = r.GetInt32("ID_role"), Title = r.GetString("Title") });
            return list;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllRoles error: {ex.Message}");
            return new List<Role>();
        }
    }

    public async Task<int> UpdateRoleAsync(Role role)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "UPDATE `Roles` SET Title=@t WHERE ID_role=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@t", role.Title);
            cmd.Parameters.AddWithValue("@id", role.ID_role);
            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateRole error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteRoleAsync(int roleId)
    {
        try
        {
            await using var conn = await OpenAsync();

            // если есть пользователи с этой ролью - запрещаем удаление
            const string countSql = "SELECT COUNT(*) FROM `Users` WHERE Role=@id;";
            await using (var countCmd = new MySqlCommand(countSql, conn))
            {
                countCmd.Parameters.AddWithValue("@id", roleId);
                var usersCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
                if (usersCount > 0)
                {
                    await Toast.Make($"Нельзя удалить роль - {usersCount} пользователей используют").Show();
                    return 0;
                }
            }

            const string delSql = "DELETE FROM `Roles` WHERE ID_role=@id;";
            await using var cmd = new MySqlCommand(delSql, conn);
            cmd.Parameters.AddWithValue("@id", roleId);
            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteRole error: {ex.Message}");
            return 0;
        }
    }

    #endregion

    #region Departments

    public async Task<int> CreateDepartmentAsync(Department department)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "INSERT INTO `Departments`(DepartmentName) VALUES(@n); SELECT LAST_INSERT_ID();";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", department.DepartmentName);
            department.ID_depart = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            await Toast.Make("Успешно").Show();
            return department.ID_depart;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"CreateDepartment error: {ex.Message}");
            return 0;
        }
    }

    public async Task<Department> GetDepartmentByIdAsync(int departmentId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "SELECT ID_depart, DepartmentName FROM `Departments` WHERE ID_depart=@id LIMIT 1;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", departmentId);
            await using var r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync()) return null;
            return new Department { ID_depart = r.GetInt32("ID_depart"), DepartmentName = r.GetString("DepartmentName") };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetDepartmentById error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        var list = new List<Department>();
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "SELECT ID_depart, DepartmentName FROM `Departments` ORDER BY DepartmentName;";
            await using var cmd = new MySqlCommand(sql, conn);
            await using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(new Department { ID_depart = r.GetInt32("ID_depart"), DepartmentName = r.GetString("DepartmentName") });
            return list;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"GetAllDepartments error: {ex.Message}");
            return new List<Department>();
        }
    }

    public async Task<int> UpdateDepartmentAsync(Department department)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "UPDATE `Departments` SET DepartmentName=@n WHERE ID_depart=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@n", department.DepartmentName);
            cmd.Parameters.AddWithValue("@id", department.ID_depart);
            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"UpdateDepartment error: {ex.Message}");
            return 0;
        }
    }

    public async Task<int> DeleteDepartmentAsync(int departmentId)
    {
        try
        {
            await using var conn = await OpenAsync();
            const string sql = "DELETE FROM `Departments` WHERE ID_depart=@id;";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", departmentId);
            var affected = await cmd.ExecuteNonQueryAsync();
            await Toast.Make("Успешно").Show();
            return affected;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"DeleteDepartment error: {ex.Message}");
            return 0;
        }
    }

    #endregion
}
