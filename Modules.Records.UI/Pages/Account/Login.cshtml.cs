using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Modules.Records.UI.Demo;
using Shared.Infrastructure.Identity;
using System.ComponentModel.DataAnnotations;

namespace Modules.Records.UI.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl ?? Url.Content("~/");
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
            return Page();

        var result = await _signInManager.PasswordSignInAsync(
            Input.Email, Input.Password, isPersistent: false, lockoutOnFailure: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }

    public async Task<IActionResult> OnPostDemoAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var demoUser = await _userManager.FindByEmailAsync(DemoUserDefaults.Email);
        if (demoUser is null)
        {
            ModelState.AddModelError(string.Empty, "Demo account is not available.");
            return Page();
        }

        // Bypass the password challenge — the credentials live server-side and
        // are not surfaced to the client. Use an absolute 12h ticket so demo
        // sessions don't accumulate indefinitely.
        var properties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.Add(DemoUserDefaults.SessionLifetime),
            AllowRefresh = false,
        };

        await _signInManager.SignInAsync(demoUser, properties);
        return LocalRedirect(returnUrl);
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
