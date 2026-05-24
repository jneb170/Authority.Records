namespace Modules.Records.Application.ReadModels;

/// <summary>
/// Read model for the polymorphic Narrative↔owner link. Lets a query list the
/// narratives attached to a given owner (e.g. an Incident) without touching the
/// aggregate tables.
/// </summary>
public sealed class NarrativeLinkReadModel
{
    public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }
    public Guid NarrativeId { get; private set; }
    public string OwnerType { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }

    private NarrativeLinkReadModel() { } // EF Core materialization

    public static NarrativeLinkReadModel Create(
        Guid id,
        Guid jurisdictionId,
        Guid narrativeId,
        string ownerType,
        Guid ownerId,
        int displayOrder,
        DateTime linkedAtUtc)
    {
        return new NarrativeLinkReadModel
        {
            Id             = id,
            JurisdictionId = jurisdictionId,
            NarrativeId    = narrativeId,
            OwnerType      = ownerType,
            OwnerId        = ownerId,
            DisplayOrder   = displayOrder,
            LinkedAtUtc    = linkedAtUtc,
        };
    }
}
