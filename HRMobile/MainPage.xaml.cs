using HRMobile.Services;
namespace HRMobile;
public partial class MainPage : ContentPage
{
    private readonly IAuthService _authService;
    public MainPage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        Loaded += async (_, _) => await UpdateStateAsync();
    }
    private async Task UpdateStateAsync()
    {
        var token = await _authService.GetIdTokenAsync();
        var signedIn = !string.IsNullOrWhiteSpace(token);
        StatusLabel.Text = signedIn ? "You are signed in." : "You are not signed in.";
        LoginButton.IsVisible = !signedIn;
        LogoutButton.IsVisible = signedIn;
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        StatusLabel.Text = "Connecting to AuthServer...";
        try
        {
            if (await _authService.LoginAsync()) { StatusLabel.Text = "Signed in successfully."; await UpdateStateAsync(); }
        }
        catch (TaskCanceledException) { StatusLabel.Text = "Sign-in was cancelled."; }
        catch (Exception ex) { await DisplayAlertAsync("Sign-in failed", ex.Message, "OK"); StatusLabel.Text = "You are not signed in."; }
        finally { LoginButton.IsEnabled = true; }
    }
    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _authService.LogoutAsync();
        await UpdateStateAsync();
    }
}