using OAuthConfig;
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
    
    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> LoginAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            DiscoveryDocument discovery;

            try
            {
                discovery = await _httpClient.GetFromJsonAsync<DiscoveryDocument>(
                    OAuthConfiguration.DiscoveryEndpoint,
                    cancellationToken);

                if (discovery is null ||
                    string.IsNullOrWhiteSpace(discovery.AuthorizationEndpoint) ||
                    string.IsNullOrWhiteSpace(discovery.TokenEndpoint))
                {
                    throw new InvalidOperationException(
                        "The AuthServer discovery document is incomplete.");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP error: {ex}");

                for (var inner = ex.InnerException;
                     inner is not null;
                     inner = inner.InnerException)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"Inner error: {inner.GetType().FullName}: {inner.Message}");
                }

                throw new InvalidOperationException(
                    "Failed to launch the system browser for login. [Discovery]", ex);
            }

            var verifier = CreateRandomValue();
            var challenge = CreateCodeChallenge(verifier);
            var state = CreateRandomValue();

            var parameters = new Dictionary<string, string>
            {
                ["client_id"] = OAuthConfiguration.ClientId,
                ["redirect_uri"] = OAuthConfiguration.RedirectUri,
                ["response_type"] = "code",
                ["scope"] = OAuthConfiguration.Scope,
                ["state"] = state,
                ["code_challenge"] = challenge,
                ["code_challenge_method"] = "S256"
            };

            var authorizationUrl =
                discovery.AuthorizationEndpoint + "?" +
                string.Join("&", parameters.Select(p =>
                    $"{Uri.EscapeDataString(p.Key)}=" +
                    $"{Uri.EscapeDataString(p.Value)}"));

            var callback = await WebAuthenticator.Default.AuthenticateAsync(
                new Uri(authorizationUrl),
                new Uri(OAuthConfiguration.RedirectUri));

            cancellationToken.ThrowIfCancellationRequested();

            if (!callback.Properties.TryGetValue("state", out var returnedState) ||
                !CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(state),
                    Encoding.UTF8.GetBytes(returnedState)))
            {
                throw new InvalidOperationException(
                    "OAuth state validation failed.");
            }

            if (!callback.Properties.TryGetValue("code", out var code) ||
                string.IsNullOrWhiteSpace(code))
            {
                throw new InvalidOperationException(
                    "AuthServer did not return an authorization code.");
            }

            var requestContent = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["client_id"] = OAuthConfiguration.ClientId,
                ["redirect_uri"] = OAuthConfiguration.RedirectUri,
                ["code"] = code,
                ["code_verifier"] = verifier
            });

            System.Diagnostics.Debug.WriteLine($"Token endpoint: {discovery.TokenEndpoint}");
            try
            {
                // Log the outgoing request body for debugging (may contain sensitive data).
                System.Diagnostics.Debug.WriteLine("Token request body: " + await requestContent.ReadAsStringAsync());
            }
            catch { }

            using var tokenResponse = await _httpClient.PostAsync(
                discovery.TokenEndpoint,
                requestContent,
                cancellationToken);

            var responseBody = await tokenResponse.Content.ReadAsStringAsync(
                cancellationToken);

            System.Diagnostics.Debug.WriteLine($"Token exchange HTTP {(int)tokenResponse.StatusCode} {tokenResponse.ReasonPhrase}; response body length: {responseBody?.Length ?? 0}");

            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Token exchange failed " +
                    $"({(int)tokenResponse.StatusCode}): {responseBody}");
            }

            var tokens = JsonSerializer.Deserialize<TokenResponse>(
                responseBody);

            if (tokens is null ||
                string.IsNullOrWhiteSpace(tokens.AccessToken))
            {
                throw new InvalidOperationException(
                    "AuthServer returned no access token.");
            }

            await SecureStorage.Default.SetAsync(
                AccessTokenKey, tokens.AccessToken);

            if (!string.IsNullOrWhiteSpace(tokens.RefreshToken))
            {
                await SecureStorage.Default.SetAsync(
                    RefreshTokenKey, tokens.RefreshToken);
            }

            if (!string.IsNullOrWhiteSpace(tokens.IdToken))
            {
                await SecureStorage.Default.SetAsync(
                    IdTokenKey, tokens.IdToken);
            }

        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Login failed. See inner exception for details.", ex);
        }
        return true;
    }

    public Task<string?> GetAccessTokenAsync() =>
        SecureStorage.Default.GetAsync(AccessTokenKey);

    public async Task LogoutAsync()
    {
        try
        {
            var logoutUrl = new Uri(new(OAuthConfiguration.Authority), "/Account/Logout");

            await WebAuthenticator.Default.AuthenticateAsync(
                new WebAuthenticatorOptions
                {
                    Url = logoutUrl,
                    CallbackUrl = new(OAuthConfiguration.RedirectUri),
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
        }
    }

    private static string CreateRandomValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string CreateCodeChallenge(string verifier)
    {
        var bytes = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));

        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private sealed class DiscoveryDocument
    {
        [System.Text.Json.Serialization.JsonPropertyName(
            "authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName(
            "token_endpoint")]
        public string? TokenEndpoint { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("issuer")]
        public string? Issuer { get; set; }
    }
}