using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

public sealed class MugshotLink : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid MugshotId { get; private set; }
    public string OwnerType { get; private set; } = string.Empty;
    public Guid OwnerId { get; private set; }
    public bool IsPrimary { get; private set; }
    public int DisplayOrder { get; private set; }
    public DateTime LinkedAtUtc { get; private set; }
    public Guid LinkedByUserId { get; private set; }

    private MugshotLink()
    {
    }

    public MugshotLink(
        Guid jurisdictionId,
        Guid mugshotId,
        string ownerType,
        Guid ownerId,
        Guid linkedByUserId,
        bool isPrimary,
        int displayOrder)
    {
        if (!MugshotOwnerTypes.IsSupported(ownerType))
        {
            throw new InvalidOperationException($"Unsupported mugshot owner type '{ownerType}'.");
        }

        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        MugshotId = mugshotId;
        OwnerType = ownerType;
        OwnerId = ownerId;
        LinkedByUserId = linkedByUserId;
        LinkedAtUtc = DateTime.UtcNow;
        IsPrimary = isPrimary;
        DisplayOrder = displayOrder;

        AddDomainEvent(new MugshotLinkedToOwnerDomainEvent(
            Id,
            MugshotId,
            JurisdictionId,
            OwnerType,
            OwnerId,
            IsPrimary,
            DisplayOrder));
    }

    public void SetPrimary(bool isPrimary)
    {
        if (IsPrimary == isPrimary)
        {
            return;
        }

        IsPrimary = isPrimary;
        AddDomainEvent(new MugshotOwnerPrimaryChangedDomainEvent(
            Id,
            MugshotId,
            JurisdictionId,
            OwnerType,
            OwnerId,
            IsPrimary,
            DisplayOrder));
    }

    public void Unlink(Guid userId)
    {
        AddDomainEvent(new MugshotUnlinkedFromOwnerDomainEvent(
            Id,
            MugshotId,
            JurisdictionId,
            OwnerType,
            OwnerId));
    }
}
