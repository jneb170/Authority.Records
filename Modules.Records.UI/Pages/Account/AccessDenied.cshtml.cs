using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Modules.Records.UI.Pages.Account;

/// <summary>
/// Shown when a signed-in user requests a page their role doesn't allow
/// (the cookie handler's AccessDeniedPath). Previously this redirected to the
/// login page, which looked like being signed out.
/// </summary>
[AllowAnonymous]
public class AccessDeniedModel : PageModel
{
    public void OnGet()
    {
    }
}
