using Modules.Records.Domain.Common.Primitives;

namespace Modules.Records.Domain.DomainEvents;

public sealed record NameRestoredDomainEvent(
    Guid NameId,
    Guid UserId) : DomainEvent;
