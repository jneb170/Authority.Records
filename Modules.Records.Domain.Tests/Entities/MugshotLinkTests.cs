using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class MugshotLinkTests
{
    [Fact]
    public void Constructor_WithSupportedOwnerType_CreatesLinkAndRaisesEvent()
    {
        var jurisdictionId = Guid.NewGuid();
        var mugshotId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var linkedBy = Guid.NewGuid();

        var link = new MugshotLink(
            jurisdictionId,
            mugshotId,
            MugshotOwnerTypes.Name,
            ownerId,
            linkedBy,
            isPrimary: true,
            displayOrder: 0);

        Assert.Equal(jurisdictionId, link.JurisdictionId);
        Assert.Equal(mugshotId, link.MugshotId);
        Assert.Equal(ownerId, link.OwnerId);
        Assert.True(link.IsPrimary);

        var createdEvent = Assert.Single(link.DomainEvents.OfType<MugshotLinkedToOwnerDomainEvent>());
        Assert.Equal(mugshotId, createdEvent.MugshotId);
        Assert.Equal(MugshotOwnerTypes.Name, createdEvent.OwnerType);
        Assert.Equal(ownerId, createdEvent.OwnerId);
        Assert.True(createdEvent.IsPrimary);
    }

    [Fact]
    public void SetPrimary_WhenValueChanges_RaisesPrimaryChangedEvent()
    {
        var link = new MugshotLink(
            Guid.NewGuid(),
            Guid.NewGuid(),
            MugshotOwnerTypes.Arrest,
            Guid.NewGuid(),
            Guid.NewGuid(),
            isPrimary: false,
            displayOrder: 2);

        link.ClearDomainEvents();

        link.SetPrimary(true);

        Assert.True(link.IsPrimary);
        var evt = Assert.Single(link.DomainEvents.OfType<MugshotOwnerPrimaryChangedDomainEvent>());
        Assert.True(evt.IsPrimary);
        Assert.Equal(MugshotOwnerTypes.Arrest, evt.OwnerType);
    }

    [Fact]
    public void Unlink_RaisesUnlinkedDomainEvent()
    {
        var link = new MugshotLink(
            Guid.NewGuid(),
            Guid.NewGuid(),
            MugshotOwnerTypes.Name,
            Guid.NewGuid(),
            Guid.NewGuid(),
            isPrimary: false,
            displayOrder: 1);

        link.ClearDomainEvents();

        link.Unlink(Guid.NewGuid());

        Assert.Single(link.DomainEvents.OfType<MugshotUnlinkedFromOwnerDomainEvent>());
    }

    [Fact]
    public void Constructor_WithUnsupportedOwnerType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new MugshotLink(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Incident",
            Guid.NewGuid(),
            Guid.NewGuid(),
            isPrimary: false,
            displayOrder: 0));
    }
}
