using HRMobile.Services;

namespace HRMobile.Components;

public partial class AppHeader : ContentView
{
    private readonly IAuthService _authService;

    public AppHeader(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private void SignOutButton_Clicked(object sender, EventArgs e)
    {

    }

    private async void SignInButton_Clicked(object sender, EventArgs e)
    {
        await _authService.LoginAsync();
    }
}