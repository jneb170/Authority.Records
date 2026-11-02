using System.Security.Claims;

namespace Modules.Records.Application.Common;

/// <summary>
/// Shared logic for deciding whether an authenticated principal is the public
/// "Try the demo" account. The demo user is matched by email because the legacy
/// <c>Demo</c> role was removed — the configured <c>Demo:LoginEmail</c> (the
/// account the public button signs in) is the source of truth, with a fallback
/// constant for hosts/tests that don't set it.
/// </summary>
public static class DemoUserMatching
{
    /// <summary>Config key holding the email the public demo button signs in.</summary>
    public const string LoginEmailConfigKey = "Demo:LoginEmail";

    /// <summary>Fallback when <see cref="LoginEmailConfigKey"/> is unset (matches the seeder default).</summary>
    public const string DefaultDemoEmail = "demo@authorityrecords.dev";

    /// <summary>Normalizes the configured value (or falls back to the default demo email).</summary>
    public static string ResolveDemoEmail(string? configuredValue)
        => string.IsNullOrWhiteSpace(configuredValue) ? DefaultDemoEmail : configuredValue.Trim();

    /// <summary>
    /// User claim (stored in AspNetUserClaims, so it is part of every principal the claims factory builds)
    /// marking a per-visitor demo account created by "Explore the live demo".
    /// </summary>
    public const string DemoVisitorClaimType = "ar:demo-visitor";

    public static bool IsDemoPrincipal(ClaimsPrincipal? principal, string demoEmail)
    {
        if (principal?.Identity?.IsAuthenticated != true)
            return false;

        if (principal.HasClaim(c => c.Type == DemoVisitorClaimType))
            return true;

        // UserName == Email in this app, and the base claims factory puts the
        // username in ClaimTypes.Name; check Email too in case that changes.
        var name = principal.FindFirst(ClaimTypes.Name)?.Value;
        var email = principal.FindFirst(ClaimTypes.Email)?.Value;

        return IsDemoEmail(name, demoEmail) || IsDemoEmail(email, demoEmail);
    }

    /// <summary>True when <paramref name="email"/> is the shared demo account's email.</summary>
    public static bool IsDemoEmail(string? email, string demoEmail)
        => !string.IsNullOrWhiteSpace(email) && string.Equals(email.Trim(), demoEmail, StringComparison.OrdinalIgnoreCase);
}
