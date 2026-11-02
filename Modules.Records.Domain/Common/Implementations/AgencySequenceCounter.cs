namespace Modules.Records.Domain.Common.Implementations;

/// <summary>
/// Tracks an annual auto-incrementing sequence counter per agency and counter key.
/// Used to generate unique formatted record numbers (e.g. IncidentNum).
/// Not an AggregateRoot — no domain events; uses RowVersion for optimistic concurrency.
/// </summary>
public sealed class AgencySequenceCounter
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string CounterKey { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public long NextValue { get; private set; }
    public byte[] RowVersion { get; private set; } = Array.Empty<byte>();

    private AgencySequenceCounter() { } // EF

    public AgencySequenceCounter(Guid jurisdictionId, Guid agencyId, string counterKey, int year)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        CounterKey = counterKey;
        Year = year;
        NextValue = 0;
    }

    /// <summary>
    /// Moves the counter forward so the next value is greater than <paramref name="lastUsed"/>. Never moves it
    /// back. Used when records numbered outside the generator (imports, demo data) already exist.
    /// </summary>
    public bool EnsureAtLeast(long lastUsed)
    {
        if (NextValue >= lastUsed)
            return false;
        NextValue = lastUsed;
        return true;
    }

    /// <summary>Increments the counter and returns the new value.</summary>
    public long Increment()
    {
        NextValue++;
        return NextValue;
    }
}
