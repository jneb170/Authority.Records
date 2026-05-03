namespace Modules.Records.UI.Demo;

/// <summary>
/// Shared constants for the demo account: identity, tenant labels, and
/// session lifetime. Referenced by the seeder and the login flow so the
/// values stay in lockstep.
/// </summary>
public static class DemoUserDefaults
{
    public const string Role = "Demo";

    public const string Email = "demo@authorityrecords.dev";

    /// <summary>Hard-coded password the seeder sets and the login button uses.</summary>
    public const string Password = "Demo@1234";

    public const string FirstName = "Demo";
    public const string LastName  = "User";

    /// <summary>
    /// Admin account scoped to the Demo Police Department. Use this to fix
    /// data, update settings, or manage the demo agency. Not surfaced on the
    /// login screen — sign in manually with these credentials.
    /// </summary>
    public const string AdminEmail = "admin@demo.authorityrecords.dev";
    public const string AdminPassword = "DemoAdmin@1234";
    public const string AdminFirstName = "Demo";
    public const string AdminLastName = "Admin";

    public const string JurisdictionName  = "Demo State";
    public const string JurisdictionState = "DM";
    public const string JurisdictionCode  = "DEMO";

    public const string AgencyName = "Demo Police Department";
    public const string AgencyCode = "DPD";

    /// <summary>How long a Try-the-demo cookie lasts before the user must sign in again.</summary>
    public static readonly TimeSpan SessionLifetime = TimeSpan.FromHours(12);
}
