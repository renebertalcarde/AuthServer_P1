using HRMobile.Services;
using HRMobile.ViewModels;

namespace HRMobile.Components;

public partial class AppHeader : ContentView
{
    private readonly IAuthService _authService;
    private readonly UserViewModel _user;

    public AppHeader(IAuthService authService, UserViewModel user)
    {
        InitializeComponent();
        _authService = authService;
        _user = user;

        BindingContext = _user;
    }

    private async void SignOutButton_Clicked(object sender, EventArgs e)
    {
        await _authService.LogoutAsync();
    }

    private async void SignInButton_Clicked(object sender, EventArgs e)
    {
        await _authService.LoginAsync();
    }
}