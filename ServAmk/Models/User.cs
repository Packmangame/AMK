namespace ServAmk.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Fio { get; set; } = "";
        public string Login { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public DateTime Birthday { get; set; }

        public Guid RoleId { get; set; }
        public Guid DepartmentId { get; set; }
    }
}
