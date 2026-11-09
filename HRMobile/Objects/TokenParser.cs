using System.Text;
using System.Text.Json;

namespace HRMobile.Objects;

public class TokenParser
{
    public string UserName { get; }
    public string CompanyName { get; }        

    public bool IsSignedIn { get; }
    private JsonElement? _el;

    public TokenParser(string? token)
    {
        IsSignedIn = !string.IsNullOrEmpty(token);

        _el = GetRootElement(token);
        if (_el == null) return;

        UserName = 
            GetClaim("preferred_username") ??
            GetClaim("unique_name") ??
            GetClaim("name") ??
            GetClaim("email") ??
            GetClaim("sub") ??
            "{Unknown}";
        CompanyName = 
            GetClaim("tenant_name") ??
            GetClaim("company_name") ??
            GetClaim("tenant_code") ??
            "{Unknown}";
    }

    private JsonElement? GetRootElement(string? _token)
    {
        var parts = _token?.Split('.') ?? [];
        if (parts.Length == 3)
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            using var doc = JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload)));
            return doc.RootElement.Clone();
        }
        return null;
    }
    private string? GetClaim(string name) => 
        _el.Value.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;

}