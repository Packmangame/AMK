using System.Text.RegularExpressions;

namespace AMK.Validation
{
    public static class Validators
    {
        private static readonly Regex EmailRegex = new(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex PhoneRegex = new(@"^[+]?\d[\d\s\-()]{8,}$", RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return EmailRegex.IsMatch(email.Trim());
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return PhoneRegex.IsMatch(phone.Trim());
        }

        public static bool IsValidLogin(string login)
        {
            if (string.IsNullOrWhiteSpace(login)) return false;
            login = login.Trim();
            return login.Length >= 3 && login.Length <= 32;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            return password.Trim().Length >= 3;
        }
    }
}
