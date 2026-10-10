using AuthServer_P1.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer_P1.Controllers;

[Route("account")]
public sealed class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;

    public AccountController(
        SignInManager<IdentityUser> signInManager)
    {
        _signInManager = signInManager;
    }

    [Route("login")]
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };

        return View(model);
    }

    [Route("login")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

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

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();

        return RedirectToAction("Login");
    }
}