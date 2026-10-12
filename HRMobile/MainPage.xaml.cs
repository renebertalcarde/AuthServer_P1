using HRMobile.Services;

namespace HRMobile;

public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;

    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
    }

    private async void OnLoginClicked(
        object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        StatusLabel.Text = "Connecting to AuthServer...";

        try
        {
            var success = await _authService.LoginAsync();

            if (success)
            {
                StatusLabel.Text = "Signed in successfully.";
                LoginButton.IsVisible = false;
                LogoutButton.IsVisible = true;
            }
        }
        catch (TaskCanceledException)
        {
            StatusLabel.Text = "Sign-in was cancelled.";
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "Sign-in failed",
                ex.Message,
                "OK");

            StatusLabel.Text = "You are not signed in.";
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async void OnLogoutClicked(
        object sender, EventArgs e)
    {
        await _authService.LogoutAsync();

        StatusLabel.Text = "You are not signed in.";
        LoginButton.IsVisible = true;
        LogoutButton.IsVisible = false;
    }
}