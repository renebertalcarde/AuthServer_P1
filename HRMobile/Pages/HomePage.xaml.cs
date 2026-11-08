using HRMobile.Components;

namespace HRMobile.Pages;

public partial class HomePage : ContentPage
{
    public HomePage(AppHeader appHeader)
    {
        InitializeComponent();
        PageLayout.Header = appHeader;
    }

    private async void OnEmployeesClicked(
        object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//employees");
    }
}