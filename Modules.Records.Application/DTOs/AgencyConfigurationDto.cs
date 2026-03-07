namespace Modules.Records.Application.DTOs;

public sealed record AgencyConfigurationDto(
    Guid Id,
    string Key,
    string Value,
    DateTime? UpdatedAtUtc);
