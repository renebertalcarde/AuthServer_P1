namespace HRMobile.Services.Platforms;

public interface IPlatformAuthBrowser
{
    Task<IDictionary<string, string>> AuthenticateAsync(
        Uri authorizationUri,
        Uri callbackUri,
        CancellationToken cancellationToken = default);
}