using MediatR;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.DomainEvents;

namespace Modules.Records.Application.Citations.DomainEventHandlers;

public sealed class CitationProjectionHandler :
    INotificationHandler<CitationCreatedDomainEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public CitationProjectionHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CitationCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var readModel = CitationReadModel.Create(
            id: notification.CitationId,
            jurisdictionId: notification.JurisdictionId,
            agencyId: notification.AgencyId,
            description: notification.Description,
            issueDate: notification.IssueDate,
            createdAtUtc: notification.OccurredOnUtc);

        _dbContext.CitationReadModels.Add(readModel);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
