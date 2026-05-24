using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Polymorphic link between a <see cref="Narrative"/> and an owning record
/// (Incident/Arrest/Citation, and future modules). Mirrors MugshotLink so the
/// attachment mechanism is reusable across modules. A narrative may be linked to
/// more than one owner; an owner may have many narratives.
/// </summary>
public sealed class NarrativeLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid NarrativeId { get; private set; }
    public string OwnerType { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private NarrativeLink() { }

    public NarrativeLink(
        Guid jurisdictionId,
        Guid narrativeId,
        string ownerType,
        Guid ownerId,
        Guid linkedByUserId,
        int displayOrder)
    {
        if (!NarrativeOwnerTypes.IsSupported(ownerType))
            throw new InvalidOperationException($"Unsupported narrative owner type '{ownerType}'.");

        Id             = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        NarrativeId    = narrativeId;
        OwnerType      = ownerType;
        OwnerId        = ownerId;
        LinkedByUserId = linkedByUserId;
        LinkedAtUtc    = DateTime.UtcNow;
        DisplayOrder   = displayOrder;

        AddDomainEvent(new NarrativeLinkedToOwnerDomainEvent(
            Id, NarrativeId, JurisdictionId, OwnerType, OwnerId, DisplayOrder));
    }

    public void Unlink(Guid userId)
    {
        AddDomainEvent(new NarrativeUnlinkedFromOwnerDomainEvent(
            Id, NarrativeId, JurisdictionId, OwnerType, OwnerId, userId));
    }
}
