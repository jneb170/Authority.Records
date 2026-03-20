using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Citations;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Commands.UpdateCitationDetails;

public sealed class UpdateCitationDetailsHandler : IRequestHandler<UpdateCitationDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateCitationDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateCitationDetailsCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var citation = await _dbContext.Citations
            .FirstOrDefaultAsync(c =>
                c.Id == request.CitationId &&
                c.JurisdictionId == jurisdictionId,
                cancellationToken)
            ?? throw new InvalidOperationException("Citation not found.");

        Name? name = null;
        if (request.DefendantNameId.HasValue)
        {
            name = await _dbContext.Names
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == request.DefendantNameId.Value && n.JurisdictionId == jurisdictionId, cancellationToken)
                ?? throw new InvalidOperationException("Linked name not found.");
        }

        citation.SetLocation(request.LocationId, _modificationContext);
        var nameChanged = citation.DefendantNameId != request.DefendantNameId;
        citation.UpdateDetails(request.Description, request.IssueDate, request.CourtId, request.CitationNum, request.DefendantNameId, _modificationContext);

        var snapshot = await _dbContext.CitationNameSnapshots
            .FirstOrDefaultAsync(s => s.CitationId == citation.Id && s.JurisdictionId == jurisdictionId, cancellationToken);

        if (request.AtTimeOfName is not null)
        {
            if (snapshot is null)
            {
                _dbContext.CitationNameSnapshots.Add(
                    CitationNameSnapshotBuilder.CreateFromInput(
                        citation,
                        name?.Id,
                        name?.RecordNumber,
                        request.AtTimeOfName,
                        _tenantProvider.GetUserId()));
            }
            else
            {
                CitationNameSnapshotBuilder.UpdateFromInput(snapshot, name?.Id, name?.RecordNumber, request.AtTimeOfName, _tenantProvider.GetUserId());
            }
        }
        else if (name is not null && (nameChanged || snapshot is null))
        {
            var snapshotLocations = await CitationNameSnapshotBuilder.LoadSnapshotLocationsAsync(_dbContext, name, cancellationToken);
            if (snapshot is null)
            {
                _dbContext.CitationNameSnapshots.Add(
                    CitationNameSnapshotBuilder.CreateFromName(
                        citation,
                        name,
                        snapshotLocations.PrimaryLocation,
                        snapshotLocations.SecondaryLocation,
                        _tenantProvider.GetUserId()));
            }
            else
            {
                CitationNameSnapshotBuilder.RefreshFromName(
                    snapshot,
                    name,
                    snapshotLocations.PrimaryLocation,
                    snapshotLocations.SecondaryLocation,
                    _tenantProvider.GetUserId());
            }
        }

        await CitationSupplementalDataWriter.ApplyOfficerProfileAsync(
            _dbContext,
            citation,
            request.OfficerProfile,
            cancellationToken);

        await CitationSupplementalDataWriter.ApplyTexasDetailsAsync(
            _dbContext,
            citation,
            request.TexasDetails,
            cancellationToken);

        await CitationSupplementalDataWriter.ApplyVehicleAsync(
            _dbContext,
            citation,
            request.Vehicle,
            cancellationToken);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
