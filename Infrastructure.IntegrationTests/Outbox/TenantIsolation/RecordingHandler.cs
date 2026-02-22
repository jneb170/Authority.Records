using MediatR;
using Modules.Records.Domain.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IntegrationTests.Outbox.TenantIsolation
{
    /// <summary>
    /// Records tenant and aggregate information when domain events are processed.
    /// Used for testing tenant isolation in the outbox pattern.
    /// </summary>
    internal sealed class RecordingHandler : INotificationHandler<TestTenantIsolationDomainEvent>
    {
        private readonly ITenantProvider _tenantProvider;

        /// <summary>
        /// Collection of processed events with their associated tenant and aggregate IDs.
        /// </summary>
        public static readonly List<(Guid TenantId, Guid AggregateId)> 
            Processed = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="RecordingHandler"/> class.
        /// </summary>
        /// <param name="tenantProvider">The tenant provider to retrieve current tenant context.</param>
        public RecordingHandler(ITenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }

        /// <summary>
        /// Handles the domain event by recording the current tenant and aggregate ID.
        /// </summary>
        /// <param name="notification">The domain event to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task Handle(TestTenantIsolationDomainEvent notification, CancellationToken cancellationToken)
        {
            var currentTenant = _tenantProvider.GetJurisdictionId();

            Processed.Add((currentTenant, notification.AggregateId));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Clears all recorded processed events.
        /// </summary>
        public static void Clear() => Processed.Clear();
    }
}
