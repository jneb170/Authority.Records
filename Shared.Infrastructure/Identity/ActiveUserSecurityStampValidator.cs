using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Identity;

/// <summary>
/// Cookie security-stamp validator that, besides the usual stamp comparison, rejects
/// principals whose user can no longer sign in (e.g. deactivated, see
/// <see cref="ActiveUserConfirmation"/>).
/// </summary>
/// <remarks>
/// Deactivation also rotates the security stamp, which on its own invalidates the cookie.
/// The extra <see cref="SignInManager{TUser}.CanSignInAsync"/> check covers a user whose
/// <c>IsActive</c> flag changed without a stamp rotation (e.g. edited directly in the database).
/// </remarks>
public sealed class ActiveUserSecurityStampValidator : SecurityStampValidator<ApplicationUser>
{
    /// <summary>
    /// How often an authenticated session is re-checked against the user store (cookie
    /// security-stamp validation and Blazor circuit revalidation). Bounds how long a
    /// deactivated user keeps access. Identity's default is 30 minutes.
    /// </summary>
    public static readonly TimeSpan ValidationInterval = TimeSpan.FromMinutes(1);

    public ActiveUserSecurityStampValidator(
        IOptions<SecurityStampValidatorOptions> options,
        SignInManager<ApplicationUser> signInManager,
        ILoggerFactory logger)
        : base(options, signInManager, logger)
    {
    }

    protected override async Task<ApplicationUser?> VerifySecurityStamp(ClaimsPrincipal? principal)
    {
        var user = await base.VerifySecurityStamp(principal);
        if (user is null)
            return null;

        return await SignInManager.CanSignInAsync(user) ? user : null;
    }
}
