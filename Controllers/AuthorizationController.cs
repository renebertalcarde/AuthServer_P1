using AuthServer.Models;
using AuthServer.Persistence;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System.Security.Claims;

namespace AuthServer.Controllers;

[Route("connect")]
public sealed class AuthorizationController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly AuthDbContext _dbContext;

    public AuthorizationController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        AuthDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
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
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var tenantIdValue = User.FindFirst("tenant_id")?.Value;

        // Never trust a tenant claim by itself. Confirm that the selected
        // company and the user's membership are still active before issuing tokens.
        var membership = int.TryParse(tenantIdValue, out var tenantId)
            ? await _dbContext.UserCompanies
                .Include(item => item.Company)
                .SingleOrDefaultAsync(item =>
                    item.UserId == userId &&
                    item.CompanyId == tenantId &&
                    item.IsActive &&
                    item.Company.IsActive)
            : null;
        if (membership is null)
        {
            var companies = await _dbContext.UserCompanies
               .Where(m =>
                   m.UserId == userId &&
                   m.IsActive &&
                   m.Company.IsActive)
               .OrderBy(m => m.Company.Name)
               .Select(m => new CompanyOptionViewModel
               {
                   Id = m.CompanyId,
                   Name = m.Company.Name,
                   Code = m.Company.Code
               })
               .ToListAsync();

            var authentication = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            var isPersistent = authentication.Properties?.IsPersistent ?? false;
            await _signInManager.SignInWithClaimsAsync(
                user,
                isPersistent,
                [
                    new Claim("tenant_id", companies[0].Id.ToString()),
                    new Claim("tenant_code", companies[0].Code)
                ]);
            var reloadUrl = $"{Request.Path}{Request.QueryString}";
            return Redirect(reloadUrl);
        }

        var principal = await _signInManager.CreateUserPrincipalAsync(user);

        principal.SetClaim(
            OpenIddictConstants.Claims.Subject,
            userId);

        // Expose the Identity username as a standard OIDC profile claim.
        principal.SetClaim("preferred_username", user.UserName ?? userId);

        principal.SetClaim("tenant_id", membership.CompanyId.ToString());
        principal.SetClaim("tenant_code", membership.Company.Code);
        principal.SetClaim("tenant_name", membership.Company.Name);

        principal.SetScopes(request.GetScopes());

        // Only tokens explicitly requested for the tenant API receive its audience.
        if (principal.HasScope("tenant_api"))
        {
            principal.SetResources("authserver-p1-api");
        }

        principal.SetDestinations(claim => claim.Type switch
        {
            OpenIddictConstants.Claims.Subject =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],

            OpenIddictConstants.Claims.Name
                when claim.Subject.HasScope(
                    OpenIddictConstants.Permissions.Scopes.Profile) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],

            OpenIddictConstants.Claims.Email
                when claim.Subject.HasScope(
                    OpenIddictConstants.Permissions.Scopes.Email) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],

            "preferred_username"
                when claim.Subject.HasScope(
                    OpenIddictConstants.Scopes.Profile) =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],

            "tenant_id" or "tenant_code" =>
                [
                    OpenIddictConstants.Destinations.AccessToken
                ],

            "tenant_name" =>
                [
                    OpenIddictConstants.Destinations.AccessToken,
                    OpenIddictConstants.Destinations.IdentityToken
                ],

            _ => Array.Empty<string>()
        });

        return SignIn(
            principal,
           // new AuthenticationProperties { RedirectUri = Request.Query["redirect_uri"] },
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }
}