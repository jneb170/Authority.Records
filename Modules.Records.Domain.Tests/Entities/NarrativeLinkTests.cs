using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class NarrativeLinkTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestNarrativeId = Guid.NewGuid();
    private static readonly Guid TestOwnerId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Theory]
    [InlineData(NarrativeOwnerTypes.Incident)]
    [InlineData(NarrativeOwnerTypes.Arrest)]
    [InlineData(NarrativeOwnerTypes.Citation)]
    public void Constructor_WithSupportedOwnerType_SetsProperties_AndRaisesEvent(string ownerType)
    {
        var link = new NarrativeLink(
            TestJurisdictionId, TestNarrativeId, ownerType, TestOwnerId, TestUserId, displayOrder: 2);

        Assert.NotEqual(Guid.Empty, link.Id);
        Assert.Equal(TestJurisdictionId, link.JurisdictionId);
        Assert.Equal(TestNarrativeId, link.NarrativeId);
        Assert.Equal(ownerType, link.OwnerType);
        Assert.Equal(TestOwnerId, link.OwnerId);
        Assert.Equal(TestUserId, link.LinkedByUserId);
        Assert.Equal(2, link.DisplayOrder);
        Assert.IsAssignableFrom<IMultiTenant>(link);

        var evt = Assert.Single(link.DomainEvents.OfType<NarrativeLinkedToOwnerDomainEvent>());
        Assert.Equal(link.Id, evt.LinkId);
        Assert.Equal(TestNarrativeId, evt.NarrativeId);
        Assert.Equal(ownerType, evt.OwnerType);
        Assert.Equal(TestOwnerId, evt.OwnerId);
    }

    [Fact]
    public void Constructor_WithUnsupportedOwnerType_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => new NarrativeLink(
            TestJurisdictionId, TestNarrativeId, "Mugshot", TestOwnerId, TestUserId, displayOrder: 0));
    }

    [Fact]
    public void Unlink_RaisesUnlinkedEvent()
    {
        var link = new NarrativeLink(
            TestJurisdictionId, TestNarrativeId, NarrativeOwnerTypes.Incident, TestOwnerId, TestUserId, 0);
        link.ClearDomainEvents();

        link.Unlink(TestUserId);

        var evt = Assert.Single(link.DomainEvents.OfType<NarrativeUnlinkedFromOwnerDomainEvent>());
        Assert.Equal(link.Id, evt.LinkId);
        Assert.Equal(TestNarrativeId, evt.NarrativeId);
        Assert.Equal(TestUserId, evt.UserId);
    }
}
