namespace Modules.Records.Application.Abstractions;

/// <summary>
/// Restricts a MediatR request to users in at least one of the given roles. Enforced by
/// <c>AuthorizationBehavior</c> for every sender, so a page's [Authorize] attribute is not the
/// only guard (issue #125).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class RequiresRoleAttribute(params string[] roles) : Attribute
{
    public IReadOnlyList<string> Roles { get; } = roles;
}

/// <summary>
/// The request generates fake records and is refused unless the host runs in demo mode
/// (<c>Demo:Enabled</c>), so it can never write test data into a real agency's records (#125).
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class DemoOnlyAttribute : Attribute;

/// <summary>Whether the host runs as the public demo. Bound from <c>Demo:Enabled</c> by the UI host.</summary>
public sealed class DemoModeOptions
{
    public bool Enabled { get; set; }
}
