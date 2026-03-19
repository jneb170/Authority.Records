using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Incidents.Commands.SaveIncidentPage;

public sealed class SaveIncidentPageHandler : IRequestHandler<SaveIncidentPageCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public SaveIncidentPageHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(SaveIncidentPageCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        var chargeIdsToAdd = NormalizeAdds(request.ChargeIdsToAdd, request.ChargeIdsToRemove);
        var chargeIdsToRemove = NormalizeRemoves(request.ChargeIdsToAdd, request.ChargeIdsToRemove);

        var incident = await _dbContext.Incidents
            .FirstOrDefaultAsync(i => i.Id == request.IncidentId && i.JurisdictionId == jurisdictionId, cancellationToken)
            ?? throw new InvalidOperationException("Incident not found.");

        incident.SetLocation(request.LocationId, _modificationContext);
        incident.UpdateDetails(request.Details, request.OccurredOn, _modificationContext);

        if (chargeIdsToRemove.Count > 0)
        {
            var linksToRemove = await _dbContext.IncidentChargeLinks
                .Where(l => l.IncidentId == request.IncidentId && chargeIdsToRemove.Contains(l.ChargeId))
                .ToListAsync(cancellationToken);

            foreach (var link in linksToRemove)
            {
                link.Unlink(userId);
            }

            _dbContext.IncidentChargeLinks.RemoveRange(linksToRemove);
        }

        if (chargeIdsToAdd.Count > 0)
        {
            var charges = await _dbContext.Charges
                .Where(c => chargeIdsToAdd.Contains(c.Id))
                .ToListAsync(cancellationToken);

            if (charges.Count != chargeIdsToAdd.Count)
                throw new InvalidOperationException("One or more charges were not found.");

            if (charges.Any(c => !c.IsActive))
                throw new InvalidOperationException("Only active charges can be linked.");

            var existingChargeIds = await _dbContext.IncidentChargeLinks
                .Where(l => l.IncidentId == request.IncidentId && chargeIdsToAdd.Contains(l.ChargeId))
                .Select(l => l.ChargeId)
                .ToListAsync(cancellationToken);

            foreach (var chargeId in chargeIdsToAdd.Except(existingChargeIds))
            {
                _dbContext.IncidentChargeLinks.Add(
                    new IncidentChargeLink(jurisdictionId, request.IncidentId, chargeId, userId));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static HashSet<Guid> NormalizeAdds(
        IReadOnlyCollection<Guid>? idsToAdd,
        IReadOnlyCollection<Guid>? idsToRemove)
    {
        var adds = idsToAdd?.Where(id => id != Guid.Empty).ToHashSet() ?? [];
        if (idsToRemove is null)
            return adds;

        adds.ExceptWith(idsToRemove.Where(id => id != Guid.Empty));
        return adds;
    }

    private static HashSet<Guid> NormalizeRemoves(
        IReadOnlyCollection<Guid>? idsToAdd,
        IReadOnlyCollection<Guid>? idsToRemove)
    {
        var removes = idsToRemove?.Where(id => id != Guid.Empty).ToHashSet() ?? [];
        if (idsToAdd is null)
            return removes;

        removes.ExceptWith(idsToAdd.Where(id => id != Guid.Empty));
        return removes;
    }
}
