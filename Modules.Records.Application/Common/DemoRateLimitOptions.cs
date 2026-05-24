namespace Modules.Records.Application.Common;

/// <summary>
/// Abuse limits applied only to the shared public "Try the demo" account so a
/// hacker can't fill the server with junk. Bound from the <c>Demo:RateLimit</c>
/// configuration section; the defaults apply when unconfigured.
/// </summary>
public sealed class DemoRateLimitOptions
{
    public const string SectionName = "Demo:RateLimit";

    /// <summary>Max records the demo account may create per <see cref="WindowMinutes"/>, across all aggregates.</summary>
    public int MaxCreatesPerWindow { get; set; } = 30;

    /// <summary>Rolling window, in minutes, the create count is measured over.</summary>
    public int WindowMinutes { get; set; } = 60;

    /// <summary>
    /// Max serialized size (UTF-8 bytes) of a single write the demo account may
    /// submit. Caps how much can be stuffed into one save — chiefly the
    /// unbounded free-text narrative/description fields. 64 KB is ~10x a very
    /// verbose real record and far above any legitimate single save.
    /// </summary>
    public int MaxBytesPerWrite { get; set; } = 64 * 1024;
}
