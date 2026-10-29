using System.Text.Json;

namespace SharedLib.Extensions;

public static class IdentityToken
{
    public static string GetUserName(this JsonElement el) =>
        GetClaim(el, "preferred_username") ??
        GetClaim(el, "unique_name") ?? 
        GetClaim(el, "name") ??
        GetClaim(el, "email") ?? 
        GetClaim(el, "sub") ?? 
        "{Unknown user}";

    public static  string GetCompanyName(this JsonElement el) =>
        GetClaim(el, "tenant_name") ??
        GetClaim(el, "company_name") ?? 
        GetClaim(el, "tenant_code") ??
        "{Unknown Client}";

    private static string? GetClaim(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) ? (value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString()) : null;
}