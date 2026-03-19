using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.SaveArrestPage;

public sealed class SaveArrestPageHandler : IRequestHandler<SaveArrestPageCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public SaveArrestPageHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(SaveArrestPageCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        var incidentIdsToAdd = NormalizeAdds(request.IncidentIdsToAdd, request.IncidentIdsToRemove);
        var incidentIdsToRemove = NormalizeRemoves(request.IncidentIdsToAdd, request.IncidentIdsToRemove);
        var chargeIdsToAdd = NormalizeAdds(request.ChargeIdsToAdd, request.ChargeIdsToRemove);
        var chargeIdsToRemove = NormalizeRemoves(request.ChargeIdsToAdd, request.ChargeIdsToRemove);

        // Ensure the primary incident is always linked: force it into the add-set
        // and prevent it from being removed, regardless of what the caller submitted.
        if (request.PrimaryIncidentId.HasValue)
        {
            incidentIdsToAdd.Add(request.PrimaryIncidentId.Value);
            incidentIdsToRemove.Remove(request.PrimaryIncidentId.Value);
        }

        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a => a.Id == request.ArrestId && a.JurisdictionId == jurisdictionId, cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        var nameExists = await _dbContext.Names
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.NameId && n.JurisdictionId == jurisdictionId, cancellationToken);

        if (!nameExists)
            throw new InvalidOperationException("Linked name not found.");

        if (request.PrimaryIncidentId.HasValue)
        {
            var primaryIncidentExists = await _dbContext.Incidents
                .AsNoTracking()
                .AnyAsync(i => i.Id == request.PrimaryIncidentId.Value && i.JurisdictionId == jurisdictionId, cancellationToken);

            if (!primaryIncidentExists)
                throw new InvalidOperationException("Primary incident not found.");
        }

        arrest.SetLocation(request.LocationId, _modificationContext);
        arrest.UpdateDetails(
            request.NameId,
            request.ArrestedAt,
            request.ArrestTypeId,
            request.ArrestNum,
            request.PrimaryIncidentId,
            _modificationContext);

        if (incidentIdsToRemove.Count > 0)
        {
            var linksToRemove = await _dbContext.IncidentArrestLinks
                .Where(l => l.ArrestId == request.ArrestId && incidentIdsToRemove.Contains(l.IncidentId))
                .ToListAsync(cancellationToken);

            foreach (var link in linksToRemove)
            {
                link.Unlink(userId);
            }

            _dbContext.IncidentArrestLinks.RemoveRange(linksToRemove);
        }

        if (incidentIdsToAdd.Count > 0)
        {
            var incidents = await _dbContext.Incidents
                .Where(i => incidentIdsToAdd.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            if (incidents.Count != incidentIdsToAdd.Count)
                throw new InvalidOperationException("One or more linked incidents were not found.");

            var existingIncidentIds = await _dbContext.IncidentArrestLinks
                .Where(l => l.ArrestId == request.ArrestId && incidentIdsToAdd.Contains(l.IncidentId))
                .Select(l => l.IncidentId)
                .ToListAsync(cancellationToken);

            foreach (var incidentId in incidentIdsToAdd.Except(existingIncidentIds))
            {
                _dbContext.IncidentArrestLinks.Add(
                    new IncidentArrestLink(jurisdictionId, incidentId, request.ArrestId, userId));
            }
        }

        if (chargeIdsToRemove.Count > 0)
        {
            var linksToRemove = await _dbContext.ArrestChargeLinks
                .Where(l => l.ArrestId == request.ArrestId && chargeIdsToRemove.Contains(l.ChargeId))
                .ToListAsync(cancellationToken);

            foreach (var link in linksToRemove)
            {
                link.Unlink(userId);
            }

            _dbContext.ArrestChargeLinks.RemoveRange(linksToRemove);
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

            var existingChargeIds = await _dbContext.ArrestChargeLinks
                .Where(l => l.ArrestId == request.ArrestId && chargeIdsToAdd.Contains(l.ChargeId))
                .Select(l => l.ChargeId)
                .ToListAsync(cancellationToken);

            foreach (var chargeId in chargeIdsToAdd.Except(existingChargeIds))
            {
                _dbContext.ArrestChargeLinks.Add(
                    new ArrestChargeLink(jurisdictionId, request.ArrestId, chargeId, userId));
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
