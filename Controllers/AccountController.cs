using System.Security.Claims;
using AuthServer.Models;
using AuthServer.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly AuthDbContext _dbContext;

    public AccountController(
        SignInManager<IdentityUser> signInManager,
        UserManager<IdentityUser> userManager,
        AuthDbContext dbContext)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    [HttpGet("login")]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [HttpPost("login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                // Keep the original OAuth return URL intact. The authorization
                // endpoint will ask for company selection if no tenant is selected.
                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) &&
                    Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked. Please try again later.");
            }
            else if (result.IsNotAllowed)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "You are not currently allowed to sign in.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid username or password.");
            }
        }

        return View(model);
    }

    [HttpGet("select-company")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> SelectCompany(string? returnUrl = null)
    {
        var model = await BuildCompanySelectionModelAsync();
        model.ReturnUrl = IsLocalReturnUrl(returnUrl) ? returnUrl : null;

        return View("SelectCompany", model);
    }

    [HttpPost("select-company")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectCompany(CompanySelectionViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge(IdentityConstants.ApplicationScheme);
        }

        if (!ModelState.IsValid)
        {
            var returnUrl = model.ReturnUrl;
            model = await BuildCompanySelectionModelAsync();
            model.ReturnUrl = IsLocalReturnUrl(returnUrl) ? returnUrl : null;
            return View("SelectCompany", model);
        }

        var userId = await _userManager.GetUserIdAsync(user);
        var membership = await _dbContext.UserCompanies
            .Include(item => item.Company)
            .SingleOrDefaultAsync(item =>
                item.UserId == userId &&
                item.CompanyId == model.CompanyId &&
                item.IsActive &&
                item.Company.IsActive);

        if (membership is null)
        {
            ModelState.AddModelError(
                nameof(model.CompanyId),
                "That company is not available to your account.");

            var invalidModel = await BuildCompanySelectionModelAsync();
            invalidModel.ReturnUrl = IsLocalReturnUrl(model.ReturnUrl) ? model.ReturnUrl : null;
            return View("SelectCompany", invalidModel);
        }

        // Preserve the current cookie's persistence setting while replacing
        // its tenant context with a membership validated against the database.
        var authentication = await HttpContext.AuthenticateAsync(
            IdentityConstants.ApplicationScheme);
        var isPersistent = authentication.Properties?.IsPersistent ?? false;

        await _signInManager.SignInWithClaimsAsync(
            user,
            isPersistent,
            new[]
            {
                new Claim("tenant_id", membership.CompanyId.ToString()),
                new Claim("tenant_code", membership.Company.Code)
            });

        if (IsLocalReturnUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl!);
        }

        return RedirectToAction("Index", "Home");
    }

    [HttpGet("logout")]
    public IActionResult Logout(LogoutViewModel? model = null)
    {
        return View(model);
    }

    [HttpPost("logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _signInManager.SignOutAsync();
            return Redirect(SharedLib.OAuthConfiguration.RedirectUri);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("signoutfailed", ex.Message);
            return View(new LogoutViewModel { Ex = ex });
        }
    }

    private async Task<CompanySelectionViewModel> BuildCompanySelectionModelAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new CompanySelectionViewModel();
        }

        var companies = await _dbContext.UserCompanies
            .Where(membership =>
                membership.UserId == userId &&
                membership.IsActive &&
                membership.Company.IsActive)
            .OrderBy(membership => membership.Company.Name)
            .Select(membership => new CompanyOptionViewModel
            {
                Id = membership.CompanyId,
                Name = membership.Company.Name,
                Code = membership.Company.Code
            })
            .ToListAsync();

        return new CompanySelectionViewModel
        {
            Companies = companies
        };
    }

    private bool IsLocalReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl);
}