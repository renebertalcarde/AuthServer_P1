namespace HRMobile.Services;

public interface IAuthService
{
    Task<bool> LoginAsync(
        CancellationToken cancellationToken = default);

    Task<string?> GetAccessTokenAsync();

    Task LogoutAsync();
}