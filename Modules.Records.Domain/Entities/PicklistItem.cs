using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// A configurable reference-data item that belongs to a named picklist type (e.g. "ArrestType", "Court").
/// Agencies can add, edit, activate, and deactivate items. System-provided defaults are seeded on first use.
/// Items are never hard-deleted — deactivation preserves FK integrity for existing records.
/// </summary>
public sealed class PicklistItem : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }

    /// <summary>Logical grouping key (e.g. "ArrestType"). See <see cref="PicklistTypes"/>.</summary>
    public string PicklistType { get; private set; } = string.Empty;

    /// <summary>Stable internal key value (e.g. "OnView"). Used for programmatic references.</summary>
    public string Value { get; private set; } = string.Empty;

    /// <summary>User-visible display text. Agencies may customise this.</summary>
    public string Label { get; private set; } = string.Empty;

    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }

    /// <summary>True when the item was seeded from system defaults rather than created by the agency.</summary>
    public bool IsSystemDefault { get; private set; }

    private PicklistItem() { } // EF

    public PicklistItem(
        Guid jurisdictionId,
        Guid agencyId,
        string picklistType,
        string value,
        string label,
        int sortOrder,
        bool isSystemDefault = false)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        PicklistType = picklistType;
        Value = value;
        Label = label;
        SortOrder = sortOrder;
        IsActive = true;
        IsSystemDefault = isSystemDefault;

        AddDomainEvent(new PicklistItemCreatedDomainEvent(Id, JurisdictionId, AgencyId, PicklistType, Value, Label));
    }

    public void UpdateLabel(string label)
    {
        Label = label;
        AddDomainEvent(new PicklistItemUpdatedDomainEvent(Id, Label, SortOrder));
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        AddDomainEvent(new PicklistItemUpdatedDomainEvent(Id, Label, SortOrder));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new PicklistItemDeactivatedDomainEvent(Id));
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new PicklistItemActivatedDomainEvent(Id));
    }
}
