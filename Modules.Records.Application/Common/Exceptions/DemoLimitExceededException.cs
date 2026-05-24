namespace Modules.Records.Application.Common.Exceptions;

/// <summary>
/// Thrown when the shared public demo account exceeds an abuse limit (creation
/// rate or per-write size). The message is user-facing — the record pages catch
/// it and show it in their error alert.
/// </summary>
public sealed class DemoLimitExceededException : Exception
{
    public DemoLimitExceededException(string message) : base(message) { }
}
