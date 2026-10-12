using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace AuthServer.Controllers;

[Route("connect")]
public sealed class AuthorizationController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public AuthorizationController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [Route("authorize")]
    [HttpGet]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest()
            ?? throw new InvalidOperationException(
                "The OpenIddict authorization request cannot be retrieved.");

        var user = await _userManager.GetUserAsync(User);

        if (user is null)
        {
            return Challenge(
                IdentityConstants.ApplicationScheme);
        }

        var principal =
            await _signInManager.CreateUserPrincipalAsync(user);

        principal.SetClaim(
            OpenIddictConstants.Claims.Subject,
            await _userManager.GetUserIdAsync(user));

        principal.SetScopes(request.GetScopes());

        principal.SetDestinations(claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Subject =>
                new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },

            OpenIddictConstants.Claims.Name
                when claim.Subject.HasScope(
                    OpenIddictConstants.Permissions.Scopes.Profile) =>
                new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },

            OpenIddictConstants.Claims.Email
                when claim.Subject.HasScope(
                    OpenIddictConstants.Permissions.Scopes.Email) =>
                new[]
                {
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                },

            _ => Array.Empty<string>()
        });

        return SignIn(
            principal,
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}