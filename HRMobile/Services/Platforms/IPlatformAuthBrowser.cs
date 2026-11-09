namespace HRMobile.Services.Platforms;

public interface IPlatformAuthBrowser
{
    Task<Dictionary<string, string>> AuthenticateAsync(
        Uri url,
        Uri callbackUri,
        CancellationToken cancellationToken = default);
}