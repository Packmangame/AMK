namespace AMK.Services;

/// <summary>
/// В проекте схема БД уже создана и поддерживается вручную (phpMyAdmin/Timeweb).
/// Клиентское приложение НЕ ДОЛЖНО создавать/изменять таблицы.
/// Заглушка оставлена, чтобы не ломать старые вызовы.
/// </summary>
internal static class MySqlSchema
{
    public static Task EnsureCreatedAsync(string connectionString) => Task.CompletedTask;
}
