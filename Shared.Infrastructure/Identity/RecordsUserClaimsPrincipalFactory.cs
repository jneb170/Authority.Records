using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Shared.Infrastructure.Identity;

public sealed class RecordsUserClaimsPrincipalFactory
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
{
    /// <summary>Claim carrying <see cref="ApplicationUser.FullName"/> ("First Last").</summary>
    public const string FullNameClaimType = "fullname";

    public RecordsUserClaimsPrincipalFactory(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor) { }

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        // Display name for the header, dashboard greeting and demo banner. Without it the UI fell back
        // to ClaimTypes.Name, which is the email address (issue #100).
        if (!string.IsNullOrWhiteSpace(user.FullName))
            identity.AddClaim(new Claim(FullNameClaimType, user.FullName));

        // Super users manage jurisdictions/admins only — no tenant context needed
        var roles = await UserManager.GetRolesAsync(user);
        if (roles.Contains("Super"))
            return identity;

        identity.AddClaim(new Claim("jurisdiction", user.JurisdictionId.ToString()));
        identity.AddClaim(new Claim("agency", user.AgencyId.ToString()));

        return identity;
    }
}
