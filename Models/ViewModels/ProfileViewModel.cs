using AMK.Models.Users;
using AMK.Services;
using AMK.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AMK.Models.ViewModels
{
    public partial class ProfileViewModel : ObservableObject
    {
        private readonly SessionService _session;

        [ObservableProperty]
        private User currentUser;

        [ObservableProperty]
        private bool isAdmin;

        public ProfileViewModel(SessionService session)
        {
            _session = session;
            Refresh();
        }

        [RelayCommand]
        private void Refresh()
        {
            CurrentUser = _session.CurrentUser;
            IsAdmin = CurrentUser?.Role == 1 || string.Equals(CurrentUser?.RoleDetail?.Title, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        [RelayCommand]
        private async Task OpenEditProfileAsync()
        {
            //await Shell.Current.GoToAsync(nameof(AMK.Views.EditProfilePage));
        }

        [RelayCommand]
        private async Task OpenEmployeesAsync()
        {

            await Shell.Current.GoToAsync("//EditDatabase");
        }

        [RelayCommand]
        private async Task OpenCompanyNewsAdminAsync()
        {
            await Shell.Current.GoToAsync($"//EditCompNews");
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            _session.SetCurrentUser(null);
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
