using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LocationSoftDeletedDomainEvent(
    Guid LocationId,
    Guid UserId) : DomainEvent;
