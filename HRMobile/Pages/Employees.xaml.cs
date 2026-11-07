using HRMobile.ViewModels;

namespace HRMobile.Pages;

public partial class Employees : ContentPage
{
    public Employees(UserViewModel user)
    {
        InitializeComponent();
        PageLayout.User = user;
    }
}