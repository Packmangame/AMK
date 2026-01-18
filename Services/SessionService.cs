using AMK.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AMK.Services
{
    public class SessionService
    {
        private static SessionService _instance;
        public static SessionService Instance => _instance ??= new SessionService();

        public User CurrentUser { get; private set; }

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        //вывод
        //SessionService.Instance.SetCurrentUser(_user);

        //прием
        //private User _currentUser;
        //_currentUser = UserSessionService.Instance.CurrentUser;
    }
}
