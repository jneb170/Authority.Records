using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Domain.Entities;

/// <summary>
/// Per-agency configuration for a picklist type: controls whether the field is required on records.
/// </summary>
public sealed class PicklistSetting : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }

    /// <summary>Picklist type key (e.g. "ArrestType"). See <see cref="PicklistTypes"/>.</summary>
    public string PicklistType { get; private set; } = string.Empty;

    public bool IsRequired { get; private set; }

    private PicklistSetting() { } // EF

    public PicklistSetting(Guid jurisdictionId, Guid agencyId, string picklistType, bool isRequired)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        PicklistType = picklistType;
        IsRequired = isRequired;
    }

    public void SetRequired(bool isRequired)
    {
        IsRequired = isRequired;
    }
}
