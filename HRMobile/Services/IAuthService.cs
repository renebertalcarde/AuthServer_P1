namespace HRMobile.Services;
public interface IAuthService
{
    event EventHandler? IdentityChanged;
    Task<bool> LoginAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetIdTokenAsync();
    Task LogoutAsync();
}