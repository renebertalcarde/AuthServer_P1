using AuthServer.Persistence;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;

namespace LogixSys.AuthServer.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AuthDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            options.UseOpenIddict();
        });

        services.AddOpenIddict()
            .AddCore(options =>
            {
                options.UseEntityFrameworkCore()
                    .UseDbContext<AuthDbContext>();
            })
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("/connect/authorize");
                options.SetTokenEndpointUris("/connect/token");

                options.AllowAuthorizationCodeFlow();
                options.AllowRefreshTokenFlow();

                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough();

                options.RegisterScopes(
                    OpenIddictConstants.Scopes.Email,
                    OpenIddictConstants.Scopes.Profile,
                    OpenIddictConstants.Scopes.OfflineAccess,
                    "tenant_api");
            });

        return services;
    }
}