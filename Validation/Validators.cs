using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace AMK.Validation
{
    public static class Validators
    {
       
        private static readonly Regex EmailRegex = new(
            @"^[A-Z0-9._%+\-]+@[A-Z0-9\-]+(\.[A-Z0-9\-]+)*\.[A-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

       
     

        public static bool IsNotEmpty(string? value) =>
            !string.IsNullOrWhiteSpace(value);

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            email = email.Trim();
            return EmailRegex.IsMatch(email);
        }


        public static bool IsValidLogin(string? login)
        {
            if (string.IsNullOrWhiteSpace(login)) return false;
            login = login.Trim();
            return login.Length >= 3 && login.Length <= 32;
        }

        // Пароль: >=8, хотя бы 1 заглавная (RU/EN), хотя бы 1 спецсимвол
        public static bool IsValidPassword(string? password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            password = password.Trim();
            if (password.Length < 8) return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasSymbol = password.Any(c => !char.IsLetterOrDigit(c));

            return hasUpper && hasSymbol;
        }

        // Дата рождения: 16..120 лет
        public static bool IsValidBirthDate(DateTime birthday)
        {
            var today = DateTime.Today;
            if (birthday.Date > today) return false;

            int age = today.Year - birthday.Year;
            if (birthday.Date > today.AddYears(-age)) age--;

            return age >= 16 && age <= 120;
        }

        // Если дата приходит строкой: dd.MM.yyyy / dd-MM-yyyy / yyyy-MM-dd, без букв
        public static bool TryParseBirthDate(string? input, out DateTime birthday)
        {
            birthday = default;
            if (string.IsNullOrWhiteSpace(input)) return false;
            input = input.Trim();

            if (input.Any(char.IsLetter)) return false;

            string[] formats = { "dd.MM.yyyy", "dd-MM-yyyy", "yyyy-MM-dd" };
            if (!DateTime.TryParseExact(input, formats, CultureInfo.GetCultureInfo("ru-RU"), DateTimeStyles.None, out birthday))
                return false;

            return IsValidBirthDate(birthday);
        }
    }
}
