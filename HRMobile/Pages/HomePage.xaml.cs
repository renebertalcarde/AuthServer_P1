using HRMobile.ViewModels;

namespace HRMobile.Pages;

public partial class HomePage : ContentPage
{
    public HomePage(UserViewModel user)
    {
        InitializeComponent();
        PageLayout.User = user;
    }

    private async void OnEmployeesClicked(
        object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//employees");
    }
}