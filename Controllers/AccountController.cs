using AuthServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
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
    public async Task<IActionResult> Login(
        LoginViewModel model)
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
                // Return only to a local URL.
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
            return Redirect(OAuthConfig.OAuthConfiguration.RedirectUri);
        }
        catch(Exception ex)
        {
            ModelState.AddModelError("signoutfailed", ex.Message);
            return View(new LogoutViewModel { Ex = ex });
        }
    }
}