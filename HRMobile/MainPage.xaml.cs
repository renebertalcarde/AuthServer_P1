using System.Text;
using System.Text.Json;
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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        LoginButton.IsEnabled = false;
        StatusLabel.Text = "Connecting to AuthServer...";

        try
        {
            var success = await _authService.LoginAsync();

            if (success)
            {
                await ShowIdentityAsync();
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
            await DisplayAlertAsync("Sign-in failed", ex.Message, "OK");
            StatusLabel.Text = "You are not signed in.";
            ClearIdentity();
        }
        finally
        {
            LoginButton.IsEnabled = true;
        }
    }

    private async Task ShowIdentityAsync()
    {
        var idToken = await _authService.GetIdTokenAsync();

        if (string.IsNullOrWhiteSpace(idToken))
        {
            ClearIdentity();
            StatusLabel.Text = "Signed in, but no ID token was available.";
            return;
        }

        try
        {
            var parts = idToken.Split('.');
            if (parts.Length != 3)
                throw new FormatException("The ID token is not in JWT format.");

            var payload = parts[1]
                .Replace('-', '+')
                .Replace('_', '/');

            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');

            var json = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;

            var username = GetStringClaim(root, "name")
                ?? GetStringClaim(root, "preferred_username")
                ?? GetStringClaim(root, "unique_name")
                ?? GetStringClaim(root, "email")
                ?? GetStringClaim(root, "sub")
                ?? "Signed-in user";

            IdentityLabel.Text = username;
            IdentityLabel.IsVisible = true;

            var claims = root.EnumerateObject()
                .Select(property => $"{property.Name}: {FormatClaimValue(property.Value)}");

            ClaimsLabel.Text = string.Join(Environment.NewLine, claims);
            ClaimsLabel.IsVisible = true;
            ClaimsHeadingLabel.IsVisible = true;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or InvalidOperationException)
        {
            ClearIdentity();
            await DisplayAlertAsync(
                "ID token",
                "The ID token was received, but its claims could not be displayed.",
                "OK");
        }
    }

    private static string? GetStringClaim(JsonElement root, string claimName)
    {
        if (!root.TryGetProperty(claimName, out var value))
            return null;

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }

    private static string FormatClaimValue(JsonElement value) =>
        value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.GetRawText();

    private void ClearIdentity()
    {
        IdentityLabel.Text = string.Empty;
        IdentityLabel.IsVisible = false;
        ClaimsLabel.Text = string.Empty;
        ClaimsLabel.IsVisible = false;
        ClaimsHeadingLabel.IsVisible = false;
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _authService.LogoutAsync();

        StatusLabel.Text = "You are not signed in.";
        LoginButton.IsVisible = true;
        LogoutButton.IsVisible = false;
        ClearIdentity();
    }
}
