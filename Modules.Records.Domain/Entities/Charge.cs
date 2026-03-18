using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.Entities;

public sealed class Charge : AggregateRoot, IMultiTenant
{
    public Guid JurisdictionId { get; private set; }
    public Guid AgencyId { get; private set; }
    public string OffenseName { get; private set; } = string.Empty;
    public string UcrCategory { get; private set; } = string.Empty;
    public string NibrsGroup { get; private set; } = string.Empty;
    public string CrimeAgainst { get; private set; } = string.Empty;
    public string UcrCode { get; private set; } = string.Empty;
    public string ChargeLevel { get; private set; } = string.Empty;
    public string? StateClass { get; private set; }
    public bool IsCitationEligible { get; private set; }
    public bool IsActive { get; private set; }

    private Charge() { } // EF

    public Charge(
        Guid jurisdictionId,
        Guid agencyId,
        string offenseName,
        string ucrCategory,
        string nibrsGroup,
        string crimeAgainst,
        string ucrCode,
        string chargeLevel,
        string? stateClass,
        bool isCitationEligible,
        bool isActive)
    {
        Id = Guid.NewGuid();
        JurisdictionId = jurisdictionId;
        AgencyId = agencyId;
        OffenseName = offenseName;
        UcrCategory = ucrCategory;
        NibrsGroup = nibrsGroup;
        CrimeAgainst = crimeAgainst;
        UcrCode = ucrCode;
        ChargeLevel = chargeLevel;
        StateClass = stateClass;
        IsCitationEligible = isCitationEligible;
        IsActive = isActive;
    }
}
