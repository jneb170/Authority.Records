using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.DomainEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Common
{
    /// <summary>
    /// Test implementation of <see cref="IDomainEventDispatcher"/> that doesn't dispatch events.
    /// Used in tests where domain event handling is not the focus.
    /// </summary>
    internal class TestDomainEventDispatcher : IDomainEventDispatcher
    {
        public Task DispatchAsync(IDomainEvent domainEvent,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        /// <summary>
        /// No-op implementation that completes immediately without dispatching events.
        /// </summary>
        /// <param name="domainEvents">The domain events to dispatch (ignored).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A completed task.</returns>
        public Task DispatchAsync(
            IEnumerable<Modules.Records.Domain.DomainEvents.IDomainEvent> domainEvents,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
