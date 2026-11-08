using HRMobile.Components;

namespace HRMobile.Pages;

public partial class Employees : ContentPage
{
    public Employees(AppHeader appHeader)
    {
        InitializeComponent();
        PageLayout.Header = appHeader;
    }
}