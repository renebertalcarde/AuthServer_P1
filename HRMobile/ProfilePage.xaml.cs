using System.Text;
using System.Text.Json;
using HRMobile.Services;
namespace HRMobile;
public partial class ProfilePage : ContentPage
{
    private readonly IAuthService _authService;
    public ProfilePage(IAuthService authService)
    {
        InitializeComponent();
        _authService = authService;
        _authService.IdentityChanged += (_, _) => MainThread.BeginInvokeOnMainThread(async () => await LoadProfileAsync());
        Loaded += async (_, _) => await LoadProfileAsync();
    }
    private async Task LoadProfileAsync()
    {
        var token = await _authService.GetIdTokenAsync();
        if (string.IsNullOrWhiteSpace(token))
        {
            ProfileStatusLabel.Text = "Not signed in.";
            ProfileIdentityLabel.Text = string.Empty;
            ProfileClaimsLabel.Text = string.Empty;
            return;
        }
        try
        {
            var parts = token.Split('.');
            if (parts.Length != 3) throw new FormatException();
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var document = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            var root = document.RootElement;
            ProfileIdentityLabel.Text = GetClaim(root, "name") ?? GetClaim(root, "preferred_username") ?? GetClaim(root, "unique_name") ?? GetClaim(root, "email") ?? GetClaim(root, "sub") ?? "Signed-in user";
            ProfileStatusLabel.Text = "Signed in";
            ProfileClaimsLabel.Text = string.Join(Environment.NewLine, root.EnumerateObject().Select(p => $"{p.Name}: {p.Value}"));
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            ProfileStatusLabel.Text = "ID token could not be decoded.";
        }
    }
    private static string? GetClaim(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;
}