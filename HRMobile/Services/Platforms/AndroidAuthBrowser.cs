namespace HRMobile.Services.Platforms;

public sealed class AndroidAuthBrowser : IPlatformAuthBrowser
{
    public async Task<IDictionary<string, string>> AuthenticateAsync(
        Uri authorizationUri,
        Uri callbackUri,
        CancellationToken cancellationToken = default)
    {
        var result = await WebAuthenticator.Default.AuthenticateAsync(
            authorizationUri,
            callbackUri,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return result.Properties;
    }
}