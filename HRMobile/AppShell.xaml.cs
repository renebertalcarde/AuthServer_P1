using HRMobile.Services;
using SharedLib.Extensions;
using System.Text;
using System.Text.Json;
namespace HRMobile.Pages;
public partial class AppShell : Shell
{
    private readonly IAuthService _authService;
    private bool _isSignedIn;
    public AppShell(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _authService.IdentityChanged += OnIdentityChanged;
        Loaded += async (_, _) => await RefreshIdentityAsync();
        Navigating += OnShellNavigating;
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target.Location.OriginalString;
        if (_isSignedIn || !target.Contains("EmployeesPage", StringComparison.OrdinalIgnoreCase))
            return;

        // Cancel synchronously so EmployeesPage cannot appear while signed out.
        e.Cancel();
        await GoToAsync("//MainPage");
    }

    private void OnIdentityChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(async () => await RefreshIdentityAsync());
    private async Task RefreshIdentityAsync()
    {
        var token = await _authService.GetIdTokenAsync();
        _isSignedIn = !string.IsNullOrWhiteSpace(token);
        if (!_isSignedIn) { HeaderIdentityLabel.Text = "Not signed in"; return; }
        try
        {
            var parts = token?.Split('.') ?? [];
            if (parts.Length != 3) { HeaderIdentityLabel.Text = "Signed in"; return; }
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = doc.RootElement;
            var username = root.GetUserName();
            var companyName = root.GetCompanyName();

            HeaderIdentityLabel.Text = string.IsNullOrWhiteSpace(companyName)
                ? username
                : $"{username} / {companyName}";
        }
        catch (Exception ex) when (ex is FormatException or JsonException) { HeaderIdentityLabel.Text = "Signed in"; }
    }
}