using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record NameSoftDeletedDomainEvent(
    Guid NameId,
    Guid UserId) : DomainEvent;
