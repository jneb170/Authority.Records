using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Common.Policies;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class IncidentTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();
    private static readonly Guid TestUserId = Guid.NewGuid();

    private static Incident CreateIncident(
        string? incidentNum = null,
        string? localNum = null,
        string? description = null,
        string? cfsNum = null) =>
        new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = TestJurisdictionId,
            AgencyId = TestAgencyId,
            Details = new IncidentDetails
            {
                IncidentNum = incidentNum ?? "INC-2026-001",
                LocalNum = localNum ?? "LOCAL-001",
                Description = description ?? "Test Incident",
                CFSNum = cfsNum ?? "CFS-001"
            }
        });

    private static IModificationContext CreateContext(
        Guid? userId = null,
        bool canOverrideLocks = false,
        bool canModifyClosedRecords = false) =>
        new TestModificationContext(
            userId ?? TestUserId,
            canOverrideLocks,
            canModifyClosedRecords);

    private static ILifecyclePolicy<Incident> DefaultPolicy() =>
        new DefaultLifecyclePolicy<Incident>(new DefaultClosePolicy<Incident>());

    #region Constructor Tests

    [Fact]
    public void Constructor_ViaFactory_SetsJurisdictionAndAgency()
    {
        var incident = CreateIncident();

        Assert.NotEqual(Guid.Empty, incident.Id);
        Assert.Equal(TestJurisdictionId, incident.JurisdictionId);
        Assert.Equal(TestAgencyId, incident.AgencyId);
    }

    [Fact]
    public void Constructor_ViaFactory_SetsIncidentDetailsFromRequest()
    {
        var incident = CreateIncident(
            incidentNum: "INC-2026-999",
            localNum: "LOCAL-999",
            description: "Robbery",
            cfsNum: "CFS-999");

        Assert.Equal("INC-2026-999", incident.IncidentNum);
        Assert.Equal("LOCAL-999", incident.LocalNum);
        Assert.Equal("Robbery", incident.Description);
        Assert.Equal("CFS-999", incident.CFSNum);
    }

    [Fact]
    public void Constructor_InitializesDraftStatus()
    {
        var incident = CreateIncident();

        Assert.Equal(RecordStatus.Draft, incident.Status);
    }

    [Fact]
    public void Constructor_IsNotDeleted_AndNotLocked()
    {
        var incident = CreateIncident();

        Assert.False(incident.IsDeleted);
        Assert.False(incident.IsLocked);
        Assert.Null(incident.LocationId);
    }

    [Fact]
    public void Constructor_RaisesIncidentCreatedDomainEvent()
    {
        var incident = CreateIncident();

        var evt = Assert.Single(incident.DomainEvents.OfType<IncidentCreatedDomainEvent>());
        Assert.Equal(incident.Id, evt.IncidentId);
    }

    [Fact]
    public void Constructor_ImplementsIMultiTenant()
    {
        var incident = CreateIncident();

        Assert.IsAssignableFrom<IMultiTenant>(incident);
        Assert.Equal(TestJurisdictionId, incident.JurisdictionId);
        Assert.Equal(TestAgencyId, incident.AgencyId);
    }

    [Fact]
    public void Constructor_Details_ComputedProperty_ReflectsFields()
    {
        var incident = CreateIncident(
            incidentNum: "INC-001",
            localNum: "L-001",
            description: "Test",
            cfsNum: "CFS-001");

        Assert.Equal("INC-001", incident.Details.IncidentNum);
        Assert.Equal("L-001", incident.Details.LocalNum);
        Assert.Equal("Test", incident.Details.Description);
        Assert.Equal("CFS-001", incident.Details.CFSNum);
    }

    [Fact]
    public void Factory_WithEmptyIncidentNum_ThrowsDomainException()
    {
        Assert.Throws<DomainException>(() =>
            new IncidentFactory().Create(new CreateIncidentRequest
            {
                JurisdictionId = TestJurisdictionId,
                AgencyId = TestAgencyId,
                Details = new IncidentDetails
                {
                    IncidentNum = "",
                    LocalNum = ""
                }
            }));
    }

    #endregion

    #region Lifecycle Tests

    [Fact]
    public void Open_TransitionsFromDraftToOpen()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.ClearDomainEvents();

        incident.Open(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Open, incident.Status);
        var evt = Assert.Single(incident.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Incident>>());
        Assert.Equal(RecordStatus.Draft, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Open, evt.NewStatus);
    }

    [Fact]
    public void Close_TransitionsFromOpenToClosed()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.Open(context, DefaultPolicy());
        incident.ClearDomainEvents();

        incident.Close(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Closed, incident.Status);
        var evt = Assert.Single(incident.DomainEvents.OfType<LifecycleStatusChangedDomainEvent<Incident>>());
        Assert.Equal(RecordStatus.Open, evt.PreviousStatus);
        Assert.Equal(RecordStatus.Closed, evt.NewStatus);
    }

    [Fact]
    public void Archive_TransitionsFromClosedToArchived()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.Open(context, DefaultPolicy());
        incident.Close(context, DefaultPolicy());
        incident.ClearDomainEvents();

        incident.Archive(context, DefaultPolicy());

        Assert.Equal(RecordStatus.Archived, incident.Status);
    }

    [Fact]
    public void Close_FromDraft_ThrowsDomainException()
    {
        var incident = CreateIncident();
        var context = CreateContext();

        Assert.Throws<DomainException>(() => incident.Close(context, DefaultPolicy()));
    }

    [Fact]
    public void Open_FromClosedStatus_ThrowsDomainException()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.Open(context, DefaultPolicy());
        incident.Close(context, DefaultPolicy());

        Assert.Throws<DomainException>(() => incident.Open(context, DefaultPolicy()));
    }

    [Fact]
    public void Archive_FromArchivedState_ThrowsDomainException()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.Open(context, DefaultPolicy());
        incident.Close(context, DefaultPolicy());
        incident.Archive(context, DefaultPolicy());

        Assert.ThrowsAny<Exception>(() => incident.Archive(context, DefaultPolicy()));
    }

    #endregion

    #region UpdateDetails Tests

    [Fact]
    public void UpdateDetails_OnDraftRecord_UpdatesFields()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        var newDetails = new IncidentDetails
        {
            IncidentNum = "INC-NEW",
            LocalNum = "LOCAL-NEW",
            Description = "Updated description",
            CFSNum = "CFS-NEW"
        };
        var occurredOn = DateTime.UtcNow.AddDays(-2);

        incident.UpdateDetails(newDetails, occurredOn, context);

        Assert.Equal("INC-NEW", incident.IncidentNum);
        Assert.Equal("LOCAL-NEW", incident.LocalNum);
        Assert.Equal("Updated description", incident.Description);
        Assert.Equal("CFS-NEW", incident.CFSNum);
        Assert.Equal(occurredOn, incident.OccurredOn);
    }

    [Fact]
    public void UpdateDetails_RaisesIncidentDetailsUpdatedDomainEvent()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.ClearDomainEvents();

        var newDetails = new IncidentDetails
        {
            IncidentNum = "INC-002",
            LocalNum = "L-002",
            Description = "New description",
            CFSNum = ""
        };

        incident.UpdateDetails(newDetails, null, context);

        var evt = Assert.Single(incident.DomainEvents.OfType<IncidentDetailsUpdatedDomainEvent>());
        Assert.Equal(incident.Id, evt.IncidentId);
        Assert.Equal(context.UserId, evt.ModifiedBy);
    }

    [Fact]
    public void UpdateDetails_CanClearOccurredOn()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        var details = new IncidentDetails { IncidentNum = "INC-001", LocalNum = "" };
        incident.UpdateDetails(details, DateTime.UtcNow.AddDays(-1), context);

        incident.UpdateDetails(details, null, context);

        Assert.Null(incident.OccurredOn);
    }

    [Fact]
    public void UpdateDetails_WithInvalidDetails_ThrowsDomainException()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        var invalidDetails = new IncidentDetails
        {
            IncidentNum = "",
            LocalNum = ""
        };

        Assert.Throws<DomainException>(() => incident.UpdateDetails(invalidDetails, null, context));
    }

    [Fact]
    public void UpdateDetails_OnOpenRecord_RequiresLock()
    {
        var incident = CreateIncident();
        var ownerContext = CreateContext();
        incident.Open(ownerContext, DefaultPolicy());
        incident.AcquireLock(ownerContext, TimeSpan.FromMinutes(10));

        var otherContext = CreateContext(Guid.NewGuid());

        var details = new IncidentDetails { IncidentNum = "INC-002", LocalNum = "" };

        Assert.ThrowsAny<Exception>(() => incident.UpdateDetails(details, null, otherContext));
    }

    #endregion

    #region SetLocation Tests

    [Fact]
    public void SetLocation_SetsLocationId()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        var locationId = Guid.NewGuid();

        incident.SetLocation(locationId, context);

        Assert.Equal(locationId, incident.LocationId);
    }

    [Fact]
    public void SetLocation_WithNull_ClearsLocationId()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.SetLocation(Guid.NewGuid(), context);

        incident.SetLocation(null, context);

        Assert.Null(incident.LocationId);
    }

    [Fact]
    public void SetLocation_OnOpenRecord_RequiresLock()
    {
        var incident = CreateIncident();
        var ownerContext = CreateContext();
        incident.Open(ownerContext, DefaultPolicy());
        incident.AcquireLock(ownerContext, TimeSpan.FromMinutes(10));

        var otherContext = CreateContext(Guid.NewGuid());

        Assert.ThrowsAny<Exception>(() => incident.SetLocation(Guid.NewGuid(), otherContext));
    }

    #endregion

    #region SoftDelete and Restore Tests

    [Fact]
    public void SoftDelete_MarksRecordAsDeleted()
    {
        var incident = CreateIncident();

        incident.SoftDelete(TestUserId);

        Assert.True(incident.IsDeleted);
    }

    [Fact]
    public void SoftDelete_RaisesIncidentSoftDeletedDomainEvent()
    {
        var incident = CreateIncident();
        incident.ClearDomainEvents();

        incident.SoftDelete(TestUserId);

        var evt = Assert.Single(incident.DomainEvents.OfType<IncidentSoftDeletedDomainEvent>());
        Assert.Equal(incident.Id, evt.IncidentId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void Restore_ClearsDeletedState()
    {
        var incident = CreateIncident();
        incident.SoftDelete(TestUserId);

        incident.Restore(TestUserId);

        Assert.False(incident.IsDeleted);
    }

    [Fact]
    public void Restore_RaisesIncidentRestoredDomainEvent()
    {
        var incident = CreateIncident();
        incident.SoftDelete(TestUserId);
        incident.ClearDomainEvents();

        incident.Restore(TestUserId);

        var evt = Assert.Single(incident.DomainEvents.OfType<IncidentRestoredDomainEvent>());
        Assert.Equal(incident.Id, evt.IncidentId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    #endregion

    #region Lock Tests

    [Fact]
    public void AcquireLock_SetsLockedState()
    {
        var incident = CreateIncident();
        var context = CreateContext();

        incident.AcquireLock(context, TimeSpan.FromMinutes(10));

        Assert.True(incident.IsLocked);
        Assert.Equal(TestUserId, incident.LockedByUserId);
    }

    [Fact]
    public void ReleaseLock_ClearsLockedState()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.AcquireLock(context, TimeSpan.FromMinutes(10));

        incident.ReleaseLock(context);

        Assert.False(incident.IsLocked);
        Assert.Null(incident.LockedByUserId);
    }

    [Fact]
    public void AcquireLock_RaisesLockAcquiredDomainEvent()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.ClearDomainEvents();

        incident.AcquireLock(context, TimeSpan.FromMinutes(10));

        var evt = Assert.Single(incident.DomainEvents.OfType<LockAcquiredDomainEvent<Incident>>());
        Assert.Equal(incident.Id, evt.AggregateId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void ReleaseLock_RaisesLockReleasedDomainEvent()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.AcquireLock(context, TimeSpan.FromMinutes(10));
        incident.ClearDomainEvents();

        incident.ReleaseLock(context);

        var evt = Assert.Single(incident.DomainEvents.OfType<LockReleasedDomainEvent<Incident>>());
        Assert.Equal(incident.Id, evt.AggregateId);
        Assert.Equal(TestUserId, evt.UserId);
    }

    [Fact]
    public void RenewLock_ByOwner_KeepsLockAndRaisesNoEvent()
    {
        var incident = CreateIncident();
        var context = CreateContext();
        incident.AcquireLock(context, TimeSpan.FromMinutes(10));
        incident.ClearDomainEvents();

        incident.RenewLock(context);

        // Still owned by the same user, and no event was raised (renewal must not spam the
        // audit log or churn the read model).
        Assert.True(incident.IsLocked);
        Assert.Equal(TestUserId, incident.LockedByUserId);
        Assert.NotNull(incident.LockedAtUtc);
        Assert.Empty(incident.DomainEvents);
    }

    [Fact]
    public void RenewLock_ByNonOwner_Throws()
    {
        var incident = CreateIncident();
        incident.AcquireLock(CreateContext(), TimeSpan.FromMinutes(10));

        var otherUser = CreateContext(userId: Guid.NewGuid());

        var ex = Assert.Throws<DomainException>(() => incident.RenewLock(otherUser));
        Assert.Equal("record.lock.required", ex.Code);
    }

    [Fact]
    public void RenewLock_WhenNotLocked_Throws()
    {
        var incident = CreateIncident();

        var ex = Assert.Throws<DomainException>(() => incident.RenewLock(CreateContext()));
        Assert.Equal("record.lock.required", ex.Code);
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
