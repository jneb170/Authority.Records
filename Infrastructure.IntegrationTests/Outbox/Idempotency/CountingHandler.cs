using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Outbox.Idempotency
{
    internal sealed class CountingHandler : INotificationHandler<TestIdempotencyDomainEvent>
    {
        public static int ExecutionCount;

        public static void Reset() => ExecutionCount = 0;

        public Task Handle(
            TestIdempotencyDomainEvent notification,
            CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref ExecutionCount);
            return Task.CompletedTask;
        }
    }

}
