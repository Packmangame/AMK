using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Models.Users
{
    [SQLite.Table("User")]
    public class User : ObservableObject
    {
        private int _id_user;
        private string _fio;
        private DateTime _birthday;
        private int _department;
        private string _phone;
        private int _role;
        private string _login;
        private string _password;
        private string _email;
        private int? _tests;

        [PrimaryKey, AutoIncrement]
        public int ID_user 
        { 
            get => _id_user; 
            set => SetProperty(ref _id_user, value); 
        }

        [NotNull]
        public string FIO 
        { 
            get => _fio; 
            set => SetProperty(ref _fio, value); 
        }

        [NotNull]
        public DateTime Birthday 
        { 
            get => _birthday; 
            set => SetProperty(ref _birthday, value); 
        }

        [NotNull]
        public int Department 
        { 
            get => _department; 
            set => SetProperty(ref _department, value); 
        }

        public string Phone 
        { 
            get => _phone; 
            set => SetProperty(ref _phone, value); 
        }

        [NotNull]
        public int Role 
        { 
            get => _role; 
            set => SetProperty(ref _role, value); 
        }

        [NotNull]
        public string Login 
        { 
            get => _login; 
            set => SetProperty(ref _login, value); 
        }

        [NotNull]
        public string Password 
        { 
            get => _password; 
            set => SetProperty(ref _password, value); 
        }

        public string Email 
        { 
            get => _email; 
            set => SetProperty(ref _email, value); 
        }

        public int? Tests 
        { 
            get => _tests; 
            set => SetProperty(ref _tests, value); 
        }
        
        
        [Ignore]
        public Role RoleDetail { get; set; }

         [Ignore]
        public Department DepartmentDetail { get; set; }

    }
}
