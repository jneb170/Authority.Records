namespace Modules.Records.Domain.DomainEvents;

/// <summary>
/// A configuration value was created or changed. <see cref="PreviousValue"/> is null on create;
/// it is recorded so the audit log shows what a setting such as TimeZoneId was changed from (#125).
/// </summary>
public sealed record AgencyConfigurationSetDomainEvent(
    Guid AgencyConfigurationId,
    Guid JurisdictionId,
    Guid AgencyId,
    string Key,
    string Value,
    string? PreviousValue = null)
    : DomainEvent;

/// <summary>A configuration value was deleted; the agency falls back to the built-in default (#125).</summary>
public sealed record AgencyConfigurationDeletedDomainEvent(
    Guid AgencyConfigurationId,
    Guid JurisdictionId,
    Guid AgencyId,
    string Key,
    string PreviousValue,
    Guid UserId)
    : DomainEvent;
