using OpenIddict.Abstractions;
using SharedLib.OAuthConfiguration;

namespace AuthServer.Persistence;

public static class OpenIddictSeeder_Windows
{
    private static readonly IOAuthConfiguration _conf = new SharedLib.OAuthConfiguration.Windows();
        
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var manager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        var clientId = _conf.ClientId;

        var application =
            await manager.FindByClientIdAsync(clientId);

        if (application is not null)
        {
            // Preserve the existing client's redirect URIs and permissions while
            // granting the dedicated tenant API scope.
            var existingDescriptor = new OpenIddictApplicationDescriptor();
            await manager.PopulateAsync(existingDescriptor, application);
            existingDescriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + "tenant_api");
            await manager.UpdateAsync(application, existingDescriptor);
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "AuthServer P1 Windows Application",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit
        };

        // Configure the Windows callback.
        descriptor.RedirectUris.Clear();
        descriptor.RedirectUris.Add(new(_conf.SignIn_RedirectUri));

        descriptor.PostLogoutRedirectUris.Clear();
        descriptor.PostLogoutRedirectUris.Add(new(_conf.SignOut_RedirectUri));

        // Authorization Code Flow.
        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Authorization);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Token);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.EndSession);

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


        await manager.CreateAsync(descriptor);
    }
}