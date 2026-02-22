using Modules.Records.Domain.Common;
using Modules.Records.Domain.Abstractions;

namespace Infrastructure.IntegrationTests.Outbox.RetryBehavior;

internal sealed class FailingAggregate : AggregateRoot, IMultiTenant
{
    //public Guid Id { get; private set; }
    public Guid JurisdictionId { get; private set; }

    private FailingAggregate() { }

    public FailingAggregate(Guid id, Guid jurisdictionId)
    {
        Id = id;
        JurisdictionId = jurisdictionId;

        AddDomainEvent(new FailingDomainEvent(id));
    }
}
