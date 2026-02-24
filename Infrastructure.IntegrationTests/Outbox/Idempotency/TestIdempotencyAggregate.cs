using Infrastructure.IntegrationTests.Outbox.TenantIsolation;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Outbox.Idempotency
{
    internal sealed class TestIdempotencyAggregate : AggregateRoot, IMultiTenant
    {
        //public Guid Id { get; private set; }

        public Guid JurisdictionId { get; private set; }

        private TestIdempotencyAggregate() { }

        public TestIdempotencyAggregate(Guid id, Guid jurisdictionId)
        {
            Id = id;
            JurisdictionId = jurisdictionId;

            //AddDomainEvent(new TestTenantIsolationDomainEvent(id));
            AddDomainEvent(new TestIdempotencyDomainEvent(id));
        }
    }

}
