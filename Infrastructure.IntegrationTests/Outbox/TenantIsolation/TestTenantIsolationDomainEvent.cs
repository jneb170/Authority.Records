using Modules.Records.Domain.DomainEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Outbox.TenantIsolation
{
    public sealed record TestTenantIsolationDomainEvent(Guid AggregateId) : IDomainEvent;
}
