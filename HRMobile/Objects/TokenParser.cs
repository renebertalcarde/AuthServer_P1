using HRMobile.Services;
using System.Text;
using System.Text.Json;

namespace HRMobile.Objects;

public abstract class TokenParser
{
    public event EventHandler? Fetched;
    protected bool IsSignedIn => !string.IsNullOrEmpty(_token);
    protected string? _token;
    protected JsonElement? _root;
    protected string? _un, _cn;
    protected readonly IAuthService _authService;

    public TokenParser(IAuthService authService)
    {
        _authService = authService;
        Refresh();
    }

    //public void Trigger_Fetch()
    //{
    //    Fetched?.Invoke(this, EventArgs.Empty);
    //}

    public async void Refresh()
    {
        _token = await _authService.GetIdTokenAsync();
        _root = GetRootElement();    
        
        _un = 
            GetClaim("preferred_username") ??
            GetClaim("unique_name") ??
            GetClaim("name") ??
            GetClaim("email") ??
            GetClaim("sub");

        _cn =
            GetClaim("tenant_name") ??
            GetClaim("company_name") ??
            GetClaim("tenant_code");

        Fetched?.Invoke(this, EventArgs.Empty);
    }
    protected JsonElement? GetRootElement()
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
    protected string? GetClaim(string name) =>
        _root is JsonElement el && el.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;
}