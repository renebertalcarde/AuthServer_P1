using OpenIddict.Abstractions;

namespace AuthServer.Persistence;

public static class OpenIddictSeeder_Windows
{
    private const string WindowsClientId = "authserver-p1-windows";

    // Fixed loopback port for the initial implementation.
    private static readonly Uri WindowsRedirectUri =
        new("http://127.0.0.1:49152/callback");

    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();
             
        var application =
            await manager.FindByClientIdAsync(WindowsClientId);

        var descriptor = new OpenIddictApplicationDescriptor();

        if (application is not null)
        {
            await manager.PopulateAsync(descriptor, application);
        }
        else
        {
            descriptor.ClientId = WindowsClientId;
            descriptor.DisplayName = "HRMobile Windows Application";
            descriptor.ClientType =
                OpenIddictConstants.ClientTypes.Public;
            descriptor.ConsentType =
                OpenIddictConstants.ConsentTypes.Explicit;
        }

        // Configure the Windows callback.
        descriptor.RedirectUris.Clear();
        descriptor.RedirectUris.Add(WindowsRedirectUri);

        // Authorization Code Flow.
        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Authorization);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Token);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.ResponseTypes.Code);

        // Requested scopes.
        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Scopes.Profile);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Scopes.Email);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Prefixes.Scope + "tenant_api");

        // Require PKCE.
        descriptor.Requirements.Add(
            OpenIddictConstants.Requirements.Features
                .ProofKeyForCodeExchange);

        if (application is null)
        {
            await manager.CreateAsync(descriptor);
        }
        else
        {
            await manager.UpdateAsync(application, descriptor);
        }
    }
}