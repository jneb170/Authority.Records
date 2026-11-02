namespace Modules.Records.UI.Demo;

/// <summary>
/// Shared constants for the demo account: identity, tenant labels, and
/// session lifetime. Referenced by the seeder and the login flow so the
/// values stay in lockstep.
/// </summary>
public static class DemoUserDefaults
{
    public const string Email = "demo@authorityrecords.dev";

    /// <summary>Hard-coded password the seeder sets and the login button uses.</summary>
    public const string Password = "Demo@1234";

    public const string FirstName = "Demo";
    public const string LastName  = "User";

    /// <summary>
    /// Role the public demo account is given when it is created (or when it is
    /// found with no roles at all). Supervisor shows the full records workflow
    /// plus the agency tools, without user/agency management.
    /// </summary>
    public const string Role = "Supervisor";

    /// <summary>
    /// Admin account scoped to the Demo Police Department. Use this to fix
    /// data, update settings, or manage the demo agency. Not surfaced on the
    /// login screen. Its password comes from configuration
    /// (<see cref="AdminPasswordConfigKey"/>); when unset, the account gets a
    /// random password nobody knows and can only be used after a reset.
    /// </summary>
    public const string AdminEmail = "admin@demo.authorityrecords.dev";

    /// <summary>Config key (env var <c>Demo__AdminPassword</c>) holding the demo admin password.</summary>
    public const string AdminPasswordConfigKey = "Demo:AdminPassword";

    /// <summary>
    /// Password older builds hard-coded for the demo admin. It is public (it is
    /// in the repository history), so the seeder replaces it wherever it is
    /// still in effect. Never assign it.
    /// </summary>
    public const string LegacyAdminPassword = "DemoAdmin@1234";
    public const string AdminFirstName = "Demo";
    public const string AdminLastName = "Admin";

    public const string JurisdictionName  = "Demo State";
    public const string JurisdictionState = "DM";
    public const string JurisdictionCode  = "DEMO";

    public const string AgencyName = "Demo Police Department";
    public const string AgencyCode = "DPD";

    /// <summary>
    /// Email domain for per-visitor demo accounts (see <see cref="DemoVisitorAccounts"/>). Never receives mail;
    /// it only keeps visitor usernames unique and recognisable.
    /// </summary>
    public const string VisitorEmailDomain = "visitors.demo.authorityrecords.dev";

    /// <summary>Visitor accounts are named "Demo Visitor 4821".</summary>
    public const string VisitorFirstName = "Demo";
    public const string VisitorLastNamePrefix = "Visitor";

    /// <summary>How long a Try-the-demo cookie lasts before the user must sign in again.</summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(12);
}
