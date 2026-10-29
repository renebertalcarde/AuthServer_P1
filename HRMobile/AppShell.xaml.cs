using System.Text;
using System.Text.Json;
using HRMobile.Services;
namespace HRMobile;
public partial class AppShell : Shell
{
    private readonly IAuthService _authService;
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
        if (!target.Contains("EmployeesPage", StringComparison.OrdinalIgnoreCase))
            return;

        var idToken = await _authService.GetIdTokenAsync();
        if (!string.IsNullOrWhiteSpace(idToken))
            return;

        // Stop the protected page from appearing, then return to Home.
        e.Cancel();
        await GoToAsync("//MainPage");
    }

    private void OnIdentityChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(async () => await RefreshIdentityAsync());
    private async Task RefreshIdentityAsync()
    {
        var token = await _authService.GetIdTokenAsync();
        if (string.IsNullOrWhiteSpace(token)) { HeaderIdentityLabel.Text = "Not signed in"; return; }
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) { HeaderIdentityLabel.Text = "Signed in"; return; }
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = doc.RootElement;
            var username = GetClaim(root, "preferred_username") ??
                GetClaim(root, "unique_name") ?? GetClaim(root, "name") ??
                GetClaim(root, "email") ?? GetClaim(root, "sub") ?? "Signed in";
            var companyName = GetClaim(root, "tenant_name") ??
                GetClaim(root, "company_name") ?? GetClaim(root, "tenant_code");

            HeaderIdentityLabel.Text = string.IsNullOrWhiteSpace(companyName)
                ? username
                : $"{username} / {companyName}";
        }
        catch (Exception ex) when (ex is FormatException or JsonException) { HeaderIdentityLabel.Text = "Signed in"; }
    }
    private static string? GetClaim(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;
    private async void OnProfileClicked(object sender, EventArgs e) => await GoToAsync("//ProfilePage");
}