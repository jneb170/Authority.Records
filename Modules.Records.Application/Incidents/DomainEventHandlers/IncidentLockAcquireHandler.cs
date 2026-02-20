using MediatR;
using Microsoft.Extensions.Logging;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Incidents.DomainEventHandlers;

public sealed class IncidentLockAcquiredHandler : INotificationHandler<IncidentLockAcquiredDomainEvent>
{
    private readonly ILogger<IncidentLockAcquiredHandler> _logger;

    public IncidentLockAcquiredHandler(ILogger<IncidentLockAcquiredHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(IncidentLockAcquiredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Incident {IncidentId} lock acquired by user {UserId} at {Timestamp}",
            notification.IncidentId,
            notification.UserId,
            notification.OccurredOnUtc);

        return Task.CompletedTask;
    }
}
