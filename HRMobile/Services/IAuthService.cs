namespace HRMobile.Services;
public interface IAuthService
{
    event EventHandler<AuthService.IdentityChangedEventArgs>? IdentityChanged;
    Task<bool> LoginAsync(CancellationToken cancellationToken = default);
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetIdTokenAsync();
    Task LogoutAsync();
}