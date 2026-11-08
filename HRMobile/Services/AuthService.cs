using HRMobile.Objects;
using HRMobile.Services.Platforms;
using SharedLib.OAuthConfiguration;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace HRMobile.Services;

public sealed class AuthService : IAuthService
{
    private const string AccessTokenKey = "hr_access_token";
    private const string RefreshTokenKey = "hr_refresh_token";
    private const string IdTokenKey = "hr_id_token";
    private readonly HttpClient _httpClient;
    private readonly IPlatformAuthBrowser _platformAuthBrowser;
    private readonly IOAuthConfiguration _oAuthConfiguration;

    public event EventHandler? IdentityChanged;

    public AuthService(
        HttpClient httpClient, 
        IPlatformAuthBrowser platformAuthBrowser,
        IOAuthConfiguration oAuthConfiguration
        )
    {
        _httpClient = httpClient;
        _platformAuthBrowser = platformAuthBrowser;
        _oAuthConfiguration = oAuthConfiguration;
    }

    public async Task<bool> LoginAsync(CancellationToken cancellationToken = default)
    {
        DiscoveryDocument discovery;
        try
        {
            discovery = await _httpClient.GetFromJsonAsync<DiscoveryDocument>(_oAuthConfiguration.DiscoveryEndpoint, cancellationToken)
                ?? throw new InvalidOperationException("The AuthServer discovery document is incomplete.");
            if (string.IsNullOrWhiteSpace(discovery.AuthorizationEndpoint) || string.IsNullOrWhiteSpace(discovery.TokenEndpoint))
                throw new InvalidOperationException("The AuthServer discovery document is incomplete.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Discovery error: {ex}");
            throw new InvalidOperationException("Failed to retrieve AuthServer discovery information.", ex);
        }

        var verifier = CreateRandomValue();
        var challenge = CreateCodeChallenge(verifier);
        var state = CreateRandomValue();
        var parameters = new Dictionary<string, string>
        {
            ["client_id"] = _oAuthConfiguration.ClientId,
            ["redirect_uri"] = _oAuthConfiguration.RedirectUri,
            ["response_type"] = "code",
            ["scope"] = _oAuthConfiguration.Scope,
            ["state"] = state,
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256"
        };

        var authorizationUrl = discovery.AuthorizationEndpoint + "?" + string.Join("&", parameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
        var callback = await _platformAuthBrowser.AuthenticateAsync(new Uri(authorizationUrl), new Uri(_oAuthConfiguration.RedirectUri), cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        if (!callback.TryGetValue("state", out var returnedState) ||
            !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(state), Encoding.UTF8.GetBytes(returnedState)))
            throw new InvalidOperationException("OAuth state validation failed.");
        if (!callback.TryGetValue("code", out var code) || string.IsNullOrWhiteSpace(code))
            throw new InvalidOperationException("AuthServer did not return an authorization code.");

        using var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = _oAuthConfiguration.ClientId,
            ["redirect_uri"] = _oAuthConfiguration.RedirectUri,
            ["code"] = code,
            ["code_verifier"] = verifier
        });
        using var tokenResponse = await _httpClient.PostAsync(discovery.TokenEndpoint, requestContent, cancellationToken);
        var responseBody = await tokenResponse.Content.ReadAsStringAsync(cancellationToken);
        if (!tokenResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Token exchange failed ({(int)tokenResponse.StatusCode}): {responseBody}");

        var tokens = JsonSerializer.Deserialize<TokenResponse>(responseBody)
            ?? throw new InvalidOperationException("AuthServer returned an invalid token response.");
        if (string.IsNullOrWhiteSpace(tokens.AccessToken))
            throw new InvalidOperationException("AuthServer returned no access token.");
        if (_oAuthConfiguration.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("openid", StringComparer.Ordinal) &&
            string.IsNullOrWhiteSpace(tokens.IdToken))
            throw new InvalidOperationException("AuthServer returned no ID token. Verify the openid scope and token response.");

        await SecureStorage.Default.SetAsync(AccessTokenKey, tokens.AccessToken);
        if (!string.IsNullOrWhiteSpace(tokens.RefreshToken))
            await SecureStorage.Default.SetAsync(RefreshTokenKey, tokens.RefreshToken);
        if (!string.IsNullOrWhiteSpace(tokens.IdToken))
            await SecureStorage.Default.SetAsync(IdTokenKey, tokens.IdToken);

        IdentityChanged?.Invoke(this, EventArgs.Empty);
        return true;
    }

    public Task<string?> GetAccessTokenAsync() => SecureStorage.Default.GetAsync(AccessTokenKey);
    public Task<string?> GetIdTokenAsync() => SecureStorage.Default.GetAsync(IdTokenKey);

    public async Task LogoutAsync()
    {
        try
        {
            var logoutUrl = new Uri(new Uri(_oAuthConfiguration.Authority), "/Account/Logout");
            await WebAuthenticator.Default.AuthenticateAsync(new WebAuthenticatorOptions
            {
                Url = logoutUrl,
                CallbackUrl = new Uri(_oAuthConfiguration.RedirectUri)
            });
        }
        catch (TaskCanceledException)
        {
            // The user cancelled the browser flow.
        }
        finally
        {
            SecureStorage.Default.Remove(AccessTokenKey);
            SecureStorage.Default.Remove(RefreshTokenKey);
            SecureStorage.Default.Remove(IdTokenKey);
            IdentityChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private static string CreateRandomValue() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static string CreateCodeChallenge(string verifier) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.ASCII.GetBytes(verifier))).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private sealed class DiscoveryDocument
    {
        [System.Text.Json.Serialization.JsonPropertyName("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("token_endpoint")]
        public string? TokenEndpoint { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("issuer")]
        public string? Issuer { get; set; }
    }
}