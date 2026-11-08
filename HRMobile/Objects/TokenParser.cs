using HRMobile.Services;
using System.Text;
using System.Text.Json;

namespace HRMobile.Objects;

public abstract class TokenParser
{
    public event EventHandler? Fetched;
    protected bool IsSignedIn => !string.IsNullOrEmpty(_token);
    protected string? UserName =>
            GetClaim("preferred_username") ??
            GetClaim("unique_name") ??
            GetClaim("name") ??
            GetClaim("email") ??
            GetClaim("sub");
    protected string? CompanyName =>
            GetClaim("tenant_name") ??
            GetClaim("company_name") ??
            GetClaim("tenant_code");
    private string? _token;
    private JsonElement? _root;
    protected readonly IAuthService _authService;

    public TokenParser(IAuthService authService)
    {
        _authService = authService;
        Refresh();
    }

    protected async void Refresh()
    {
        _token = await _authService.GetIdTokenAsync();
        _root = GetRootElement();
        Fetched?.Invoke(this, EventArgs.Empty);
    }
    private JsonElement? GetRootElement()
    {
        var parts = _token?.Split('.') ?? [];
        if (parts.Length == 3)
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            return doc.RootElement;
        }
        return null;
    }
    private string? GetClaim(string name) =>
        _root is JsonElement el && el.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;
}