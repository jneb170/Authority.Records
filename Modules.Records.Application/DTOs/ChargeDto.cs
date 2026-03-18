namespace Modules.Records.Application.DTOs;

public sealed record ChargeDto(
    Guid Id,
    Guid JurisdictionId,
    Guid AgencyId,
    string OffenseName,
    string UcrCategory,
    string NibrsGroup,
    string CrimeAgainst,
    string UcrCode,
    string ChargeLevel,
    string? StateClass,
    bool IsCitationEligible,
    bool IsActive);
