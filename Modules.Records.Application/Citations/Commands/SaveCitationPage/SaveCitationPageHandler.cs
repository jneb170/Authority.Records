using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Commands.SaveCitationPage;

public sealed class SaveCitationPageHandler : IRequestHandler<SaveCitationPageCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public SaveCitationPageHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(SaveCitationPageCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var userId = _tenantProvider.GetUserId();

        var incidentIdsToAdd = NormalizeAdds(request.IncidentIdsToAdd, request.IncidentIdsToRemove);
        var incidentIdsToRemove = NormalizeRemoves(request.IncidentIdsToAdd, request.IncidentIdsToRemove);
        var chargeIdsToAdd = NormalizeAdds(request.ChargeIdsToAdd, request.ChargeIdsToRemove);
        var chargeIdsToRemove = NormalizeRemoves(request.ChargeIdsToAdd, request.ChargeIdsToRemove);

        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c => c.Id == request.CitationId && c.JurisdictionId == jurisdictionId, cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        citation.SetLocation(request.LocationId, _modificationContext);
        citation.UpdateDetails(
            request.Description,
            request.IssueDate,
            request.CourtId,
            request.CitationNum,
            _modificationContext);

        if (incidentIdsToRemove.Count > 0)
        {
            var linksToRemove = await _dbContext.IncidentCitationLinks
                .Where(l => l.CitationId == request.CitationId && incidentIdsToRemove.Contains(l.IncidentId))
                .ToListAsync(cancellationToken);

            foreach (var link in linksToRemove)
            {
                link.Unlink(userId);
            }

            _dbContext.IncidentCitationLinks.RemoveRange(linksToRemove);
        }

        if (incidentIdsToAdd.Count > 0)
        {
            var incidents = await _dbContext.Incidents
                .Where(i => incidentIdsToAdd.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            if (incidents.Count != incidentIdsToAdd.Count)
                throw new InvalidOperationException("One or more linked incidents were not found.");

            var existingIncidentIds = await _dbContext.IncidentCitationLinks
                .Where(l => l.CitationId == request.CitationId && incidentIdsToAdd.Contains(l.IncidentId))
                .Select(l => l.IncidentId)
                .ToListAsync(cancellationToken);

            foreach (var incidentId in incidentIdsToAdd.Except(existingIncidentIds))
            {
                _dbContext.IncidentCitationLinks.Add(
                    new IncidentCitationLink(jurisdictionId, incidentId, request.CitationId, userId));
            }
        }

        if (chargeIdsToRemove.Count > 0)
        {
            var linksToRemove = await _dbContext.CitationChargeLinks
                .Where(l => l.CitationId == request.CitationId && chargeIdsToRemove.Contains(l.ChargeId))
                .ToListAsync(cancellationToken);

            foreach (var link in linksToRemove)
            {
                link.Unlink(userId);
            }

            _dbContext.CitationChargeLinks.RemoveRange(linksToRemove);
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

            if (charges.Any(c => !c.IsCitationEligible))
                throw new InvalidOperationException("This charge is not eligible for citation use.");

            var existingChargeIds = await _dbContext.CitationChargeLinks
                .Where(l => l.CitationId == request.CitationId && chargeIdsToAdd.Contains(l.ChargeId))
                .Select(l => l.ChargeId)
                .ToListAsync(cancellationToken);

            foreach (var chargeId in chargeIdsToAdd.Except(existingChargeIds))
            {
                _dbContext.CitationChargeLinks.Add(
                    new CitationChargeLink(jurisdictionId, request.CitationId, chargeId, userId));
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
