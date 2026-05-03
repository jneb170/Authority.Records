namespace Modules.Records.Application.Common.Behaviors;

/// <summary>
/// Thrown when the Demo user attempts a write operation. The UI hides write
/// affordances for Demo, so this exception represents either a deliberate
/// bypass attempt or a path that was missed when the policy was added —
/// either way, refuse the write.
/// </summary>
public sealed class DemoWriteForbiddenException : Exception
{
    public DemoWriteForbiddenException(string commandName)
        : base($"The demo account is read-only. '{commandName}' is not permitted.")
    {
    }
}
