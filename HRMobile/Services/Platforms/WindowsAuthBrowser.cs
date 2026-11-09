namespace HRMobile.Services.Platforms;

public sealed class WindowsAuthBrowser : IPlatformAuthBrowser
{
    public async Task<Dictionary<string, string>> AuthenticateAsync(Uri authUri, Uri callbackUri, CancellationToken cancellationToken = default)
    {
        var res = await WinUIEx.WebAuthenticator.AuthenticateAsync(authUri, callbackUri, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return res.Properties;
        
    }

    //public async Task<IDictionary<string, string>> AuthenticateAsync(
    //    Uri url,
    //    Uri callbackUri,
    //    CancellationToken cancellationToken = default)
    //{
    //    using var listener = new HttpListener();

    //    // Listen on the loopback interface.
    //    var loopbackUri = $"{callbackUri.Scheme}://{callbackUri.Authority}/";
    //    listener.Prefixes.Add(loopbackUri);

    //    listener.Start();

    //    // Open the user's default browser.
    //    await Launcher.Default.OpenAsync(url);

    //    using var registration = cancellationToken.Register(
    //        () => listener.Stop());

    //    var context = await listener.GetContextAsync();

    //    if (context.Request.Url?.AbsolutePath != callbackUri.AbsolutePath)
    //    {
    //        context.Response.StatusCode = 400;
    //        context.Response.Close();

    //        throw new InvalidOperationException(
    //            "Unexpected OAuth callback path.");
    //    }

    //    var properties = context.Request.QueryString
    //        .AllKeys
    //        .Where(key => key is not null)
    //        .ToDictionary(
    //            key => key!,
    //            key => context.Request.QueryString[key!] ?? "");

    //    const string response =
    //        "<html><body><h2>Authentication successful.</h2>" +
    //        "<p>You may close this browser window.</p></body></html>";

    //    var buffer = System.Text.Encoding.UTF8.GetBytes(response);

    //    context.Response.ContentType = "text/html";
    //    context.Response.ContentLength64 = buffer.Length;

    //    await context.Response.OutputStream.WriteAsync(buffer);
    //    context.Response.Close();

    //    return properties;
    //}
}