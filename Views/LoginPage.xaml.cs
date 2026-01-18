using AMK.Models.Users;
using AMK.Services;
using Microsoft.EntityFrameworkCore;
using SQLite;
using System.Threading.Tasks;

namespace AMK.Views;

public partial class LoginPage : ContentPage
{
    //LocalizationService _localizationService;
   
    DatabaseService _databaseService= new DatabaseService();
    
    public LoginPage()
	{
		InitializeComponent();
        //_localizationService = LocalizationService.Instance;
    }
   
    private async void EnterButton_Clicked(object sender, EventArgs e)
    {
        


       
        await Shell.Current.GoToAsync($"{nameof(MainPage)}");
    }
    
    private async void CreatePolButton_Clicked(object sender, EventArgs e)
    {
       var polz= new User        
        {
            FIO = "Пользователь Системы",
            Birthday = new DateTime(1980, 1, 1),
            Department = 1,
            Phone = "+7 (999) 999-99-99",
            Role = 2,
            Login = "polz",
            Password = "polz",
            Email = "admin@amk.ru",
            Tests = 0
        };

       await _databaseService.CreateUserAsync( polz );


    }
    private async void CreateAdminButton_Clicked(object sender, EventArgs e)
    {
        var adminUser = new User
        {
            FIO = "Администратор Системы",
            Birthday = new DateTime(1980, 1, 1),
            Department = 3,
            Phone = "+7 (999) 999-99-99",
            Role = 1,
            Login = "admin",
            Password = "admin",
            Email = "admin@amk.ru",
            Tests = 0
        };
        
        await _databaseService.CreateUserAsync(adminUser);
    }
}