namespace Modules.Records.Application.Common;

/// <summary>
/// Marks a command that creates a new record and therefore counts against the
/// demo account's per-window creation cap. Apply to the aggregate
/// <c>Create*Command</c>s. Non-demo users are never affected by the cap.
/// </summary>
public interface IRateLimitedCommand;
