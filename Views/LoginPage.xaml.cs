using AMK.Services;
using AMK.Validation;
using CommunityToolkit.Maui.Alerts;
using Validators = AMK.Validation.Validators;

namespace AMK.Views;

public partial class LoginPage : ContentPage
{
    private readonly IDatabaseService _databaseService;
    
    public LoginPage(IDatabaseService databaseService)
	{
		InitializeComponent();
		_databaseService = databaseService;
        //_localizationService = LocalizationService.Instance;
    }
   
    private async void EnterButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var login = _login.Text?.Trim();
            var password = _password.Text?.Trim();

            if (!Validators.IsNotEmpty(login) || !Validators.IsNotEmpty(password))
            {
                await Toast.Make("Заполните логин и пароль").Show();
                return;
            }

           
            var user = await _databaseService.AuthenticateAsync(login!, password!);
            if (user == null)
            {
                await Toast.Make("Неверный логин или пароль").Show();
                return;
            }

            SessionService.Instance.SetCurrentUser(user);
            var appShell = (AppShell)Shell.Current;
            await Shell.Current.GoToAsync($"//{nameof(MainPage)}");
            appShell.EnableMenu();
        }
        catch (Exception ex)
        {
            await Toast.Make($"Ошибка: {ex.Message}").Show();
        }
    }


}