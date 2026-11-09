namespace HRMobile.Services.Platforms;

public sealed class AndroidAuthBrowser : IPlatformAuthBrowser
{
    public async Task<Dictionary<string, string>> AuthenticateAsync(
        Uri url,
        Uri callbackUri,
        CancellationToken cancellationToken = default)
    {
        var result = await WebAuthenticator.Default.AuthenticateAsync(
            url,
            callbackUri,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        return result.Properties;
    }
}