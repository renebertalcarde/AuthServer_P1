using OAuthConfig;
using OpenIddict.Abstractions;

namespace AuthServer.Persistence;

public static class OpenIddictSeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var applicationManager = scope.ServiceProvider
            .GetRequiredService<IOpenIddictApplicationManager>();

        const string clientId = "authserver-p1-mobile";

        // Check whether the client already exists.
        var existingApplication =
            await applicationManager.FindByClientIdAsync(clientId);

        if (existingApplication is not null)
        {
            // Preserve the existing client's redirect URIs and permissions while
            // granting the dedicated tenant API scope.
            var existingDescriptor = new OpenIddictApplicationDescriptor();
            await applicationManager.PopulateAsync(existingDescriptor, existingApplication);
            existingDescriptor.Permissions.Add(
                OpenIddictConstants.Permissions.Prefixes.Scope + "tenant_api");
            await applicationManager.UpdateAsync(existingApplication, existingDescriptor);
            return;
        }

        // Register the mobile application.
        var descriptor = new OpenIddictApplicationDescriptor
        {
            ClientId = clientId,
            DisplayName = "AuthServer P1 Mobile Application",
            ClientType = OpenIddictConstants.ClientTypes.Public,
            ConsentType = OpenIddictConstants.ConsentTypes.Explicit
        };

        // Replace this URI with your actual mobile callback URI.
        descriptor.RedirectUris.Add(
            new Uri(OAuthConfiguration.RedirectUri));

        // Allow Authorization Code Flow.
        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Authorization);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Endpoints.Token);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.ResponseTypes.Code);

        // Allow requested scopes.
        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Scopes.Profile);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Scopes.Email);

        descriptor.Permissions.Add(
            OpenIddictConstants.Permissions.Prefixes.Scope + "tenant_api");

        // Require PKCE for this public client.
        descriptor.Requirements.Add(
            OpenIddictConstants.Requirements.Features
                .ProofKeyForCodeExchange);
               

        // Save the client in the database.
        await applicationManager.CreateAsync(descriptor);
    }
}