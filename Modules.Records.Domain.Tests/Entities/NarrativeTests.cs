using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class NarrativeTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static IModificationContext CreateContext(
        Guid? userId = null,
        bool canOverrideLocks = false) =>
        new TestModificationContext(userId ?? TestUserId, canOverrideLocks);

    private static Narrative CreateTestNarrative() =>
        new(TestJurisdictionId, "Initial Report", "On arrival, officers observed...");

    #region Constructor

    [Fact]
    public void Constructor_SetsProperties_AndRaisesCreatedEvent()
    {
        var narrative = new Narrative(TestJurisdictionId, "  Initial Report  ", "Body text");

        Assert.NotEqual(Guid.Empty, narrative.Id);
        Assert.Equal(TestJurisdictionId, narrative.JurisdictionId);
        Assert.Equal("Initial Report", narrative.Title); // trimmed
        Assert.Equal("Body text", narrative.Content);
        Assert.False(narrative.IsDeleted);
        Assert.False(narrative.IsLocked);

        var evt = Assert.Single(narrative.DomainEvents.OfType<NarrativeCreatedDomainEvent>());
        Assert.Equal(narrative.Id, evt.NarrativeId);
        Assert.Equal(TestJurisdictionId, evt.JurisdictionId);
        Assert.Equal("Initial Report", evt.Title);
    }

    [Fact]
    public void Narrative_ImplementsIMultiTenant()
    {
        var narrative = CreateTestNarrative();
        Assert.IsAssignableFrom<IMultiTenant>(narrative);
        Assert.Equal(TestJurisdictionId, narrative.JurisdictionId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithEmptyTitle_Throws(string title)
    {
        var ex = Assert.Throws<DomainException>(() => new Narrative(TestJurisdictionId, title, "body"));
        Assert.Equal("narrative.title.empty", ex.Code);
    }

    [Fact]
    public void Constructor_WithTitleOverMax_Throws()
    {
        var title = new string('X', Narrative.MaxTitleLength + 1);
        var ex = Assert.Throws<DomainException>(() => new Narrative(TestJurisdictionId, title, "body"));
        Assert.Equal("narrative.title.length", ex.Code);
    }

    [Fact]
    public void Constructor_WithContentOverMax_Throws()
    {
        var content = new string('X', Narrative.MaxContentLength + 1);
        var ex = Assert.Throws<DomainException>(() => new Narrative(TestJurisdictionId, "Title", content));
        Assert.Equal("narrative.content.length", ex.Code);
    }

    [Fact]
    public void Constructor_WithContentAtMax_DoesNotThrow()
    {
        var content = new string('X', Narrative.MaxContentLength);
        var result = Record.Exception(() => new Narrative(TestJurisdictionId, "Title", content));
        Assert.Null(result);
    }

    #endregion

    #region UpdateContent

    [Fact]
    public void UpdateContent_UpdatesFields_AndRaisesEvent()
    {
        var narrative = CreateTestNarrative();
        narrative.ClearDomainEvents();

        narrative.UpdateContent("Follow-up", "Supplemental details added.", CreateContext());

        Assert.Equal("Follow-up", narrative.Title);
        Assert.Equal("Supplemental details added.", narrative.Content);

        var evt = Assert.Single(narrative.DomainEvents.OfType<NarrativeContentUpdatedDomainEvent>());
        Assert.Equal(narrative.Id, evt.NarrativeId);
        Assert.Equal("Follow-up", evt.Title);
    }

    [Fact]
    public void UpdateContent_WithEmptyTitle_Throws()
    {
        var narrative = CreateTestNarrative();
        var ex = Assert.Throws<DomainException>(
            () => narrative.UpdateContent("", "body", CreateContext()));
        Assert.Equal("narrative.title.empty", ex.Code);
    }

    [Fact]
    public void UpdateContent_ByNonOwner_WhileLocked_Throws()
    {
        var narrative = CreateTestNarrative();
        var owner = Guid.NewGuid();
        narrative.AcquireLock(CreateContext(owner), TimeSpan.FromMinutes(10));

        // A different user without override rights cannot modify the locked narrative.
        Assert.ThrowsAny<Exception>(
            () => narrative.UpdateContent("X", "y", CreateContext(Guid.NewGuid())));
    }

    #endregion

    #region Lock / SoftDelete / Restore

    [Fact]
    public void AcquireLock_MarksLockedByUser()
    {
        var narrative = CreateTestNarrative();

        narrative.AcquireLock(CreateContext(TestUserId), TimeSpan.FromMinutes(10));

        Assert.True(narrative.IsLocked);
        Assert.Equal(TestUserId, narrative.LockedByUserId);
    }

    [Fact]
    public void SoftDelete_MarksDeleted_AndRaisesEvent()
    {
        var narrative = CreateTestNarrative();

        narrative.SoftDelete(TestUserId);

        Assert.True(narrative.IsDeleted);
        Assert.Equal(TestUserId, narrative.DeletedBy);
        Assert.NotNull(narrative.DeletedAtUtc);
        Assert.Single(narrative.DomainEvents.OfType<NarrativeSoftDeletedDomainEvent>());
    }

    [Fact]
    public void Restore_ClearsDeleted_AndRaisesEvent()
    {
        var narrative = CreateTestNarrative();
        narrative.SoftDelete(TestUserId);

        narrative.Restore(TestUserId);

        Assert.False(narrative.IsDeleted);
        Assert.Null(narrative.DeletedBy);
        Assert.Null(narrative.DeletedAtUtc);
        Assert.Single(narrative.DomainEvents.OfType<NarrativeRestoredDomainEvent>());
    }

    #endregion

    #region Test IModificationContext

    private sealed class TestModificationContext : IModificationContext
    {
        public Guid UserId { get; }
        public bool CanOverrideLocks { get; }
        public bool CanModifyClosedRecords => false;
        public bool IsSystem => false;

        public TestModificationContext(Guid userId, bool canOverrideLocks = false)
        {
            UserId = userId;
            CanOverrideLocks = canOverrideLocks;
        }
    }

    #endregion
}
