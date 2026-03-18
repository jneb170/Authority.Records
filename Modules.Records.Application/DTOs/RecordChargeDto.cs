namespace Modules.Records.Application.DTOs;

public sealed record RecordChargeDto(
    Guid ChargeId,
    string OffenseName,
    string UcrCode,
    string ChargeLevel,
    string? StateClass,
    bool IsCitationEligible,
    bool IsActive,
    DateTime LinkedAtUtc);
