using MediatR;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.DomainEvents;
using Shared.Infrastructure.Persistence;
using System.Text.Json;

namespace Shared.Infrastructure.Audit;

public sealed class AuditTrailDomainEventHandler : INotificationHandler<IDomainEvent>
{
    private readonly AppDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public AuditTrailDomainEventHandler(AppDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var entry = AuditTrailEntry.Create(
            eventId: domainEvent.EventId,
            eventType: domainEvent.GetType().Name,
            occurredOnUtc: domainEvent.OccurredOnUtc,
            jurisdictionId: _tenantProvider.GetJurisdictionId(),
            payload: JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));

        _dbContext.AuditTrailEntries.Add(entry);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
