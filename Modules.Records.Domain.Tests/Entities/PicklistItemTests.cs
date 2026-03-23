using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Tests.Entities;

public sealed class PicklistItemTests
{
    private static readonly Guid TestJurisdictionId = Guid.NewGuid();
    private static readonly Guid TestAgencyId = Guid.NewGuid();

    private static PicklistItem CreatePicklistItem(
        string? picklistType = null,
        string? value = null,
        string? label = null,
        int sortOrder = 1,
        bool isSystemDefault = false) =>
        new PicklistItem(
            TestJurisdictionId,
            TestAgencyId,
            picklistType ?? "ArrestType",
            value ?? "OnView",
            label ?? "On View",
            sortOrder,
            isSystemDefault);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithRequiredFields_SetsProperties()
    {
        var item = CreatePicklistItem(
            picklistType: "ArrestType",
            value: "Warrant",
            label: "Warrant Arrest",
            sortOrder: 5);

        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(TestJurisdictionId, item.JurisdictionId);
        Assert.Equal(TestAgencyId, item.AgencyId);
        Assert.Equal("ArrestType", item.PicklistType);
        Assert.Equal("Warrant", item.Value);
        Assert.Equal("Warrant Arrest", item.Label);
        Assert.Equal(5, item.SortOrder);
    }

    [Fact]
    public void Constructor_NewItem_IsActive()
    {
        var item = CreatePicklistItem();

        Assert.True(item.IsActive);
    }

    [Fact]
    public void Constructor_WithIsSystemDefault_True_SetsFlag()
    {
        var item = CreatePicklistItem(isSystemDefault: true);

        Assert.True(item.IsSystemDefault);
    }

    [Fact]
    public void Constructor_WithIsSystemDefault_False_SetsFlag()
    {
        var item = CreatePicklistItem(isSystemDefault: false);

        Assert.False(item.IsSystemDefault);
    }

    [Fact]
    public void Constructor_IsNotDeleted()
    {
        var item = CreatePicklistItem();

        Assert.False(item.IsDeleted);
    }

    [Fact]
    public void Constructor_RaisesPicklistItemCreatedDomainEvent()
    {
        var item = CreatePicklistItem(
            picklistType: "Court",
            value: "District",
            label: "District Court");

        var evt = Assert.Single(item.DomainEvents.OfType<PicklistItemCreatedDomainEvent>());
        Assert.Equal(item.Id, evt.PicklistItemId);
        Assert.Equal(TestJurisdictionId, evt.JurisdictionId);
        Assert.Equal(TestAgencyId, evt.AgencyId);
        Assert.Equal("Court", evt.PicklistType);
        Assert.Equal("District", evt.Value);
        Assert.Equal("District Court", evt.Label);
    }

    [Fact]
    public void Constructor_ImplementsIMultiTenant()
    {
        var item = CreatePicklistItem();

        Assert.IsAssignableFrom<IMultiTenant>(item);
        Assert.Equal(TestJurisdictionId, item.JurisdictionId);
        Assert.Equal(TestAgencyId, item.AgencyId);
    }

    #endregion

    #region UpdateLabel Tests

    [Fact]
    public void UpdateLabel_ChangesLabel()
    {
        var item = CreatePicklistItem(label: "Original Label");

        item.UpdateLabel("Updated Label");

        Assert.Equal("Updated Label", item.Label);
    }

    [Fact]
    public void UpdateLabel_RaisesPicklistItemUpdatedDomainEvent()
    {
        var item = CreatePicklistItem(sortOrder: 3);
        item.ClearDomainEvents();

        item.UpdateLabel("New Label");

        var evt = Assert.Single(item.DomainEvents.OfType<PicklistItemUpdatedDomainEvent>());
        Assert.Equal(item.Id, evt.PicklistItemId);
        Assert.Equal("New Label", evt.Label);
        Assert.Equal(3, evt.SortOrder);
    }

    [Fact]
    public void UpdateLabel_PreservesSortOrder()
    {
        var item = CreatePicklistItem(sortOrder: 7);

        item.UpdateLabel("Changed Label");

        Assert.Equal(7, item.SortOrder);
    }

    #endregion

    #region UpdateSortOrder Tests

    [Fact]
    public void UpdateSortOrder_ChangesSortOrder()
    {
        var item = CreatePicklistItem(sortOrder: 1);

        item.UpdateSortOrder(10);

        Assert.Equal(10, item.SortOrder);
    }

    [Fact]
    public void UpdateSortOrder_RaisesPicklistItemUpdatedDomainEvent()
    {
        var item = CreatePicklistItem(label: "My Label", sortOrder: 1);
        item.ClearDomainEvents();

        item.UpdateSortOrder(5);

        var evt = Assert.Single(item.DomainEvents.OfType<PicklistItemUpdatedDomainEvent>());
        Assert.Equal(item.Id, evt.PicklistItemId);
        Assert.Equal("My Label", evt.Label);
        Assert.Equal(5, evt.SortOrder);
    }

    [Fact]
    public void UpdateSortOrder_PreservesLabel()
    {
        var item = CreatePicklistItem(label: "Keep This Label");

        item.UpdateSortOrder(99);

        Assert.Equal("Keep This Label", item.Label);
    }

    #endregion

    #region Deactivate / Activate Tests

    [Fact]
    public void Deactivate_SetsIsActiveToFalse()
    {
        var item = CreatePicklistItem();
        Assert.True(item.IsActive);

        item.Deactivate();

        Assert.False(item.IsActive);
    }

    [Fact]
    public void Deactivate_RaisesPicklistItemDeactivatedDomainEvent()
    {
        var item = CreatePicklistItem();
        item.ClearDomainEvents();

        item.Deactivate();

        var evt = Assert.Single(item.DomainEvents.OfType<PicklistItemDeactivatedDomainEvent>());
        Assert.Equal(item.Id, evt.PicklistItemId);
    }

    [Fact]
    public void Activate_SetsIsActiveToTrue()
    {
        var item = CreatePicklistItem();
        item.Deactivate();
        Assert.False(item.IsActive);

        item.Activate();

        Assert.True(item.IsActive);
    }

    [Fact]
    public void Activate_RaisesPicklistItemActivatedDomainEvent()
    {
        var item = CreatePicklistItem();
        item.Deactivate();
        item.ClearDomainEvents();

        item.Activate();

        var evt = Assert.Single(item.DomainEvents.OfType<PicklistItemActivatedDomainEvent>());
        Assert.Equal(item.Id, evt.PicklistItemId);
    }

    [Fact]
    public void Deactivate_ThenActivate_TogglesIsActive()
    {
        var item = CreatePicklistItem();

        item.Deactivate();
        Assert.False(item.IsActive);

        item.Activate();
        Assert.True(item.IsActive);
    }

    #endregion
}
