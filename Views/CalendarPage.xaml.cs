namespace AMK.Views;

public partial class CalendarPage : ContentPage
{
    public CalendarPage(AMK.Models.ViewModels.CalendarViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}