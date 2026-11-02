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
    private readonly IConfiguration _configuration;

    public LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _configuration = configuration;
    }

    [BindProperty]
    public InputModel Input { get; set; } = default!;

    public string? ReturnUrl { get; set; }

    /// <summary>Project panel shown beside the sign-in card (portfolio deployments).</summary>
    public AboutPanel About { get; private set; } = AboutPanel.Disabled;

    public override void OnPageHandlerExecuting(Microsoft.AspNetCore.Mvc.Filters.PageHandlerExecutingContext context)
    {
        About = AboutPanel.FromConfiguration(_configuration);
        base.OnPageHandlerExecuting(context);
    }

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

        // Deactivated accounts come back as NotAllowed (see ActiveUserConfirmation). Identity
        // returns that before checking the password, so report it with the same generic
        // message as a bad password rather than revealing which accounts are deactivated.
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return Page();
    }

    public async Task<IActionResult> OnPostDemoAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        // The passwordless login target is configurable so an admin can point
        // "Try the demo" at any curated account (set "Demo:LoginEmail" in app
        // config / the Demo__LoginEmail environment variable). It is intentionally separate from the
        // DemoSeeder's account so the seeder never repairs/stomps the chosen
        // user's agency or roles. Falls back to the seeded demo account.
        var demoEmail = _configuration["Demo:LoginEmail"];
        if (string.IsNullOrWhiteSpace(demoEmail))
            demoEmail = DemoUserDefaults.Email;

        var demoUser = await _userManager.FindByEmailAsync(demoEmail);
        // SignInAsync skips the pre-sign-in checks PasswordSignInAsync performs, so apply
        // them here: a deactivated demo account must not be usable either.
        if (demoUser is null || !await _signInManager.CanSignInAsync(demoUser))
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

    /// <summary>
    /// Settings for the "About this project" panel, bound from the <c>About</c>
    /// configuration section (env vars <c>About__AuthorName</c>, <c>About__GitHubUrl</c>,
    /// <c>About__LinkedInUrl</c>, <c>About__WebsiteUrl</c>; <c>About__Enabled=false</c> hides it).
    /// </summary>
    public sealed record AboutPanel(bool Enabled, string? AuthorName, string? GitHubUrl, string? LinkedInUrl, string? WebsiteUrl)
    {
        public const string DefaultGitHubUrl = "https://github.com/jneb170/Authority.Records";

        public static readonly AboutPanel Disabled = new(false, null, null, null, null);

        public static AboutPanel FromConfiguration(IConfiguration configuration)
        {
            var section = configuration.GetSection("About");
            return new AboutPanel(
                Enabled: section.GetValue("Enabled", true),
                AuthorName: Clean(section["AuthorName"]),
                GitHubUrl: section["GitHubUrl"] is null ? DefaultGitHubUrl : SafeUrl(section["GitHubUrl"]),
                LinkedInUrl: SafeUrl(section["LinkedInUrl"]),
                WebsiteUrl: SafeUrl(section["WebsiteUrl"]));
        }

        private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        // Only absolute http(s) links are rendered; anything else is ignored.
        private static string? SafeUrl(string? value)
            => Uri.TryCreate(Clean(value), UriKind.Absolute, out var uri) && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                ? uri.ToString()
                : null;
    }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }
}
