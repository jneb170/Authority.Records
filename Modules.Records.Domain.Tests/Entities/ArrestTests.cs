using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class ArrestTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();
    private static readonly Guid TestNameId = Guid.NewGuid();

    private static Arrest CreateArrest(
        Guid? nameId = null,
        DateTime? arrestedAt = null,
        string? arrestNum = null,
        Guid? primaryIncidentId = null) =>
        new ArrestFactory().Create(
            TestJurisdictionId,
            TestAgencyId,
            nameId,
            arrestedAt ?? DateTime.UtcNow.AddDays(-1),
            arrestNum ?? "AR-2026-000001",
            primaryIncidentId);

    private static IModificationContext CreateContext(
        Guid? userId = null,
        bool canOverrideLocks = false,
        bool canModifyClosedRecords = false) =>
        new TestModificationContext(
            userId ?? TestUserId,
            canOverrideLocks,
            canModifyClosedRecords);

    private static ILifecyclePolicy<Arrest> DefaultPolicy() =>
        new DefaultLifecyclePolicy<Arrest>(new DefaultClosePolicy<Arrest>());

    #region Constructor Tests

    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        var arrestedAt = DateTime.UtcNow.AddDays(-1);

        var arrest = new Arrest(
            TestJurisdictionId,
            TestAgencyId,
            TestNameId,
            arrestedAt,
            "AR-2026-000001",
            null);

        Assert.NotEqual(Guid.Empty, arrest.Id);
        Assert.Equal(TestJurisdictionId, arrest.JurisdictionId);
        Assert.Equal(TestAgencyId, arrest.AgencyId);
        Assert.Equal(TestNameId, arrest.NameId);
        Assert.Equal(arrestedAt, arrest.ArrestedAt);
        Assert.Equal("AR-2026-000001", arrest.ArrestNum);
        Assert.Null(arrest.PrimaryIncidentId);
    }

    [Fact]
    public void Constructor_WithoutName_SetsNameIdToNull()
    {
        var arrest = CreateArrest(nameId: null);

        Assert.Null(arrest.NameId);
    }

    [Fact]
    public void Constructor_InitializesDraftStatus()
    {
        var arrest = CreateArrest();

        Assert.Equal(RecordStatus.Draft, arrest.Status);
    }

    [Fact]
    public void Constructor_IsNotDeleted_AndNotFinalized()
    {
        var arrest = CreateArrest();

        Assert.False(arrest.IsDeleted);
        Assert.False(arrest.IsFinalized);
    }

    [Fact]
    public void Constructor_RaisesArrestCreatedDomainEvent()
    {
        var primaryIncidentId = Guid.NewGuid();
        var arrestedAt = DateTime.UtcNow.AddDays(-2);

        var arrest = new Arrest(
            TestJurisdictionId,
            TestAgencyId,
            TestNameId,
            arrestedAt,
            "AR-001",
            primaryIncidentId);

        var evt = Assert.Single(arrest.DomainEvents.OfType<ArrestCreatedDomainEvent>());
        Assert.Equal(arrest.Id, evt.ArrestId);
        Assert.Equal(TestJurisdictionId, evt.JurisdictionId);
        Assert.Equal(TestNameId, evt.NameId);
        Assert.Equal(arrestedAt, evt.ArrestedAt);
        Assert.Equal("AR-001", evt.ArrestNum);
        Assert.Equal(primaryIncidentId, evt.PrimaryIncidentId);
    }

    [Fact]
    public void Constructor_WithPrimaryIncidentId_SetsPrimaryIncidentId()
    {
        var primaryIncidentId = Guid.NewGuid();

        var arrest = CreateArrest(primaryIncidentId: primaryIncidentId);

        Assert.Equal(primaryIncidentId, arrest.PrimaryIncidentId);
    }

    [Fact]
    public void Constructor_ImplementsIMultiTenant()
    {
        var arrest = CreateArrest();

        Assert.IsAssignableFrom<IMultiTenant>(arrest);
        Assert.Equal(TestJurisdictionId, arrest.JurisdictionId);
        Assert.Equal(TestAgencyId, arrest.AgencyId);
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Open_TransitionsFromDraftToOpen_RaisesEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.ClearDomainEvents();

        arrest.Open(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Open, arrest.Status);
        var evt = Assert.Single(arrest.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Arrest>>());
        Assert.Equal(RecordStatus.Draft, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Open, evt.NewStatus);
    }

    [Fact]
    public void Close_TransitionsFromOpenToClosed_RaisesEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.Open(context, DefaultPolicy());
        arrest.ClearDomainEvents();

        arrest.Close(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Closed, arrest.Status);
        var evt = Assert.Single(arrest.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Arrest>>());
        Assert.Equal(RecordStatus.Open, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Closed, evt.NewStatus);
    }

    [Fact]
    public void Archive_TransitionsFromClosedToArchived()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.Open(context, DefaultPolicy());
        arrest.Close(context, DefaultPolicy());
        arrest.ClearDomainEvents();

        arrest.Archive(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Archived, arrest.Status);
        var evt = Assert.Single(arrest.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Arrest>>());
        Assert.Equal(RecordStatus.Closed, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Archived, evt.NewStatus);
    }

    [Fact]
    public void Open_FromClosed_ThrowsDomainException()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.Open(context, DefaultPolicy());
        arrest.Close(context, DefaultPolicy());

        Assert.Throws<DomainException>(() => arrest.Open(context, DefaultPolicy()));
    }

    [Fact]
    public void Close_FromDraft_ThrowsDomainException()
    {
        var arrest = CreateArrest();
        var context = CreateContext();

        Assert.Throws<DomainException>(() => arrest.Close(context, DefaultPolicy()));
    }

    [Fact]
    public void Archive_FromArchivedState_DoesNotRaiseEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.Open(context, DefaultPolicy());
        arrest.Close(context, DefaultPolicy());
        arrest.Archive(context, DefaultPolicy());
        arrest.ClearDomainEvents();

        // Transitioning to Archived from Archived should throw
        Assert.ThrowsAny<Exception>(() => arrest.Archive(context, DefaultPolicy()));
    }

    #endregion

    #region Finalize Tests

    [Fact]
    public void Finalize_SetsIsFinalizedToTrue()
    {
        var arrest = CreateArrest();
        Assert.False(arrest.IsFinalized);

        arrest.Finalize();

        Assert.True(arrest.IsFinalized);
    }

    [Fact]
    public void Finalize_CanBeCalledMultipleTimes()
    {
        var arrest = CreateArrest();

        arrest.Finalize();
        arrest.Finalize();

        Assert.True(arrest.IsFinalized);
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_UpdatesAllFields()
    {
        var arrest = CreateArrest(nameId: null);
        var context = CreateContext();
        var newNameId = Guid.NewGuid();
        var newArrestedAt = DateTime.UtcNow.AddDays(-3);
        var newArrestTypeId = Guid.NewGuid();
        var newIncidentId = Guid.NewGuid();

        arrest.UpdateDetails(newNameId, newArrestedAt, newArrestTypeId, "AR-UPDATED", newIncidentId, context);

        Assert.Equal(newNameId, arrest.NameId);
        Assert.Equal(newArrestedAt, arrest.ArrestedAt);
        Assert.Equal(newArrestTypeId, arrest.ArrestTypeId);
        Assert.Equal("AR-UPDATED", arrest.ArrestNum);
        Assert.Equal(newIncidentId, arrest.PrimaryIncidentId);
    }

    [Fact]
    public void UpdateDetails_RaisesArrestDetailsUpdatedDomainEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.ClearDomainEvents();

        arrest.UpdateDetails(TestNameId, DateTime.UtcNow.AddDays(-1), null, "AR-NEW", null, context);

        var evt = Assert.Single(arrest.DomainEvents.OfType<ArrestDetailsUpdatedDomainEvent>());
        Assert.Equal(arrest.Id, evt.ArrestId);
        Assert.Equal(TestNameId, evt.NameId);
        Assert.Equal("AR-NEW", evt.ArrestNum);
        Assert.Equal(context.UserId, evt.ModifiedBy);
    }

    [Fact]
    public void UpdateDetails_CanClearNameId()
    {
        var arrest = CreateArrest(nameId: TestNameId);
        var context = CreateContext();

        arrest.UpdateDetails(null, DateTime.UtcNow.AddDays(-1), null, "AR-001", null, context);

        Assert.Null(arrest.NameId);
    }

    #endregion

    #region SetLocation Tests

    [Fact]
    public void SetLocation_SetsLocationId()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        var locationId = Guid.NewGuid();

        arrest.SetLocation(locationId, context);

        Assert.Equal(locationId, arrest.LocationId);
    }

    [Fact]
    public void SetLocation_WithNull_ClearsLocationId()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        var locationId = Guid.NewGuid();
        arrest.SetLocation(locationId, context);

        arrest.SetLocation(null, context);

        Assert.Null(arrest.LocationId);
    }

    #endregion

    #region SoftDelete and Restore Tests

    [Fact]
    public void SoftDelete_MarksRecordAsDeleted()
    {
        var arrest = CreateArrest();

        arrest.SoftDelete(TestUserId);

        Assert.True(arrest.IsDeleted);
    }

    [Fact]
    public void SoftDelete_RaisesArrestSoftDeletedDomainEvent()
    {
        var arrest = CreateArrest();
        arrest.ClearDomainEvents();

        arrest.SoftDelete(TestUserId);

        var evt = Assert.Single(arrest.DomainEvents.OfType<ArrestSoftDeletedDomainEvent>());
        Assert.Equal(arrest.Id, evt.ArrestId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void Restore_ClearsDeletedState()
    {
        var arrest = CreateArrest();
        arrest.SoftDelete(TestUserId);

        arrest.Restore(TestUserId);

        Assert.False(arrest.IsDeleted);
    }

    [Fact]
    public void Restore_RaisesArrestRestoredDomainEvent()
    {
        var arrest = CreateArrest();
        arrest.SoftDelete(TestUserId);
        arrest.ClearDomainEvents();

        arrest.Restore(TestUserId);

        var evt = Assert.Single(arrest.DomainEvents.OfType<ArrestRestoredDomainEvent>());
        Assert.Equal(arrest.Id, evt.ArrestId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    #endregion

    #region Lock Tests

    [Fact]
    public void AcquireLock_SetsLockedState()
    {
        var arrest = CreateArrest();
        var context = CreateContext();

        arrest.AcquireLock(context, TimeSpan.FromMinutes(10));

        Assert.True(arrest.IsLocked);
        Assert.Equal(TestUserId, arrest.LockedByUserId);
    }

    [Fact]
    public void ReleaseLock_ClearsLockedState()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.AcquireLock(context, TimeSpan.FromMinutes(10));

        arrest.ReleaseLock(context);

        Assert.False(arrest.IsLocked);
        Assert.Null(arrest.LockedByUserId);
    }

    [Fact]
    public void AcquireLock_RaisesLockAcquiredDomainEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.ClearDomainEvents();

        arrest.AcquireLock(context, TimeSpan.FromMinutes(10));

        var evt = Assert.Single(arrest.DomainEvents.OfType<LockAcquiredDomainEvent<Arrest>>());
        Assert.Equal(arrest.Id, evt.AggregateId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void ReleaseLock_RaisesLockReleasedDomainEvent()
    {
        var arrest = CreateArrest();
        var context = CreateContext();
        arrest.AcquireLock(context, TimeSpan.FromMinutes(10));
        arrest.ClearDomainEvents();

        arrest.ReleaseLock(context);

        var evt = Assert.Single(arrest.DomainEvents.OfType<LockReleasedDomainEvent<Arrest>>());
        Assert.Equal(arrest.Id, evt.AggregateId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    #endregion

    #region Test Infrastructure

    private sealed class TestModificationContext : IModificationContext
    {
        public Guid UserId { get; }
        public bool CanOverrideLocks { get; }
        public bool CanModifyClosedRecords { get; }
        public bool IsSystem => false;

        public TestModificationContext(Guid userId, bool canOverrideLocks, bool canModifyClosedRecords)
        {
            UserId = userId;
            CanOverrideLocks = canOverrideLocks;
            CanModifyClosedRecords = canModifyClosedRecords;
        }
    }

    #endregion
}
