using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record NameCreatedDomainEvent(
    Guid NameId,
    Guid JurisdictionId,
    string NameType,
    string LastOrBusinessName,
    string? FirstName,
    string? MiddleName) : DomainEvent;
