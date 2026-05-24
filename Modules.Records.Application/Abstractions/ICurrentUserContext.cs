namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Application-layer view of the currently authenticated user.
/// Lets handlers and pipeline behaviors check role membership without
/// taking a direct dependency on AspNetCore's HttpContext.
/// </summary>
public interface ICurrentUserContext
{
    bool IsInRole(string roleName);

    /// <summary>
    /// True when the current user is the shared public "Try the demo" account.
    /// The demo identity is matched by email (the legacy <c>Demo</c> role was
    /// removed), so this is the only runtime marker that distinguishes demo
    /// traffic. Used to apply abuse limits that don't affect real users.
    /// </summary>
    bool IsDemoUser { get; }
}
