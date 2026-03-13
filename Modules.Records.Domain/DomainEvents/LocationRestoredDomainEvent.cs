using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record LocationRestoredDomainEvent(
    Guid LocationId,
    Guid UserId) : DomainEvent;
