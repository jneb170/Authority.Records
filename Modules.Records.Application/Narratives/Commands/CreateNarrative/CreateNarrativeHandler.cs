using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Narratives.Commands.CreateNarrative;

public sealed class CreateNarrativeHandler : IRequestHandler<CreateNarrativeCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateNarrativeHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext      = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<long> Handle(CreateNarrativeCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId         = _tenantProvider.GetUserId();

        var narrative = new Narrative(jurisdictionId, request.Title, request.Content);
        _dbContext.Narratives.Add(narrative);

        var displayOrder = await _dbContext.NarrativeLinks
            .Where(l => l.JurisdictionId == jurisdictionId
                     && l.OwnerType == request.OwnerType
                     && l.OwnerId == request.OwnerId)
            .CountAsync(cancellationToken);

        var link = new NarrativeLink(
            jurisdictionId, narrative.Id, request.OwnerType, request.OwnerId, userId, displayOrder);
        _dbContext.NarrativeLinks.Add(link);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return narrative.RecordNumber;
    }
}
