using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class CitationTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void Constructor_InitializesDraftCitation()
    {
        var citation = CreateCitation();

        Assert.Equal(RecordStatus.Draft, citation.Status);
        Assert.False(citation.IsIssued);
    }

    [Fact]
    public void AcquireAndReleaseLock_RaiseGenericLockEvents()
    {
        var citation = CreateCitation();
        var context = new UserModificationContext(TestUserId);
        citation.ClearDomainEvents();

        citation.AcquireLock(context, TimeSpan.FromMinutes(10));
        citation.ReleaseLock(context);

        var acquired = Assert.Single(citation.DomainEvents.OfType<LockAcquiredDomainEvent<Citation>>());
        var released = Assert.Single(citation.DomainEvents.OfType<LockReleasedDomainEvent<Citation>>());

        Assert.Equal(citation.Id, acquired.AggregateId);
        Assert.Equal(TestUserId, acquired.UserId);
        Assert.Equal(citation.Id, released.AggregateId);
        Assert.Equal(TestUserId, released.UserId);
    }

    [Fact]
    public void Issue_RaisesCitationIssuedDomainEvent()
    {
        var citation = CreateCitation();
        var context = new UserModificationContext(TestUserId);
        citation.ClearDomainEvents();

        citation.Issue(context);

        Assert.True(citation.IsIssued);

        var issued = Assert.Single(citation.DomainEvents.OfType<CitationIssuedDomainEvent>());
        Assert.Equal(citation.Id, issued.CitationId);
        Assert.Equal(TestUserId, issued.IssuedByUserId);
    }

    [Fact]
    public void Close_ChangesStatus_WhenPolicyAllows()
    {
        var citation = CreateCitation();
        var context = new UserModificationContext(TestUserId);
        var policy = new DefaultLifecyclePolicy<Citation>(new DefaultClosePolicy<Citation>());

        citation.Open(context, policy);
        citation.ClearDomainEvents();
        citation.Close(context, policy);

        Assert.Equal(RecordStatus.Closed, citation.Status);
        var evt = Assert.Single(citation.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Citation>>());
        Assert.Equal(RecordStatus.Open, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Closed, evt.NewStatus);
    }

    private static Citation CreateCitation() =>
        new(TestJurisdictionId, TestAgencyId, "Speeding", DateTime.UtcNow.AddDays(-1), "CT-100");
}
