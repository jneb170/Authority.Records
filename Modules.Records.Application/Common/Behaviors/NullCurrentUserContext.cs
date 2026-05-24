using Modules.Records.Application.Abstractions;

namespace Modules.Records.Application.Common.Behaviors;

/// <summary>
/// Default ICurrentUserContext used when no host-specific implementation is
/// registered (tests, background services). Reports no role membership so the
/// Demo write guard does not fire in those contexts.
/// </summary>
internal sealed class NullCurrentUserContext : ICurrentUserContext
{
    public bool IsInRole(string roleName) => false;

    // Background services / seeding run as no one, never as the demo account,
    // so demo abuse limits must not fire here.
    public bool IsDemoUser => false;
}
