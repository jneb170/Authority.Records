using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;

public sealed class UpdateArrestDetailsHandler : IRequestHandler<UpdateArrestDetailsCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IModificationContext _modificationContext;

    public UpdateArrestDetailsHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _modificationContext = modificationContext;
    }

    public async Task Handle(UpdateArrestDetailsCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        var arrest = await _dbContext.Arrests
            .FirstOrDefaultAsync(a =>
                a.Id == request.ArrestId &&
                a.JurisdictionId == jurisdictionId,
                cancellationToken)
            ?? throw new InvalidOperationException("Arrest not found.");

        var name = await _dbContext.Names
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == request.NameId && n.JurisdictionId == jurisdictionId, cancellationToken)
            ?? throw new InvalidOperationException("Linked name not found.");

        if (request.PrimaryIncidentId.HasValue)
        {
            var incidentExists = await _dbContext.Incidents
                .AsNoTracking()
                .AnyAsync(i => i.Id == request.PrimaryIncidentId.Value && i.JurisdictionId == jurisdictionId, cancellationToken);

            if (!incidentExists)
                throw new InvalidOperationException("Primary incident not found.");
        }

        arrest.SetLocation(request.LocationId, _modificationContext);
        var nameChanged = arrest.NameId != request.NameId;
        arrest.UpdateDetails(request.NameId, request.ArrestedAt, request.ArrestTypeId, request.ArrestNum, request.PrimaryIncidentId, _modificationContext);

        var snapshot = await _dbContext.ArrestNameSnapshots
            .FirstOrDefaultAsync(s => s.ArrestId == arrest.Id && s.JurisdictionId == jurisdictionId, cancellationToken);

        if (request.AtTimeOfName is not null)
        {
            if (snapshot is null)
            {
                _dbContext.ArrestNameSnapshots.Add(
                    ArrestNameSnapshotBuilder.CreateFromInput(
                        arrest,
                        name.Id,
                        name.RecordNumber,
                        request.AtTimeOfName,
                        _tenantProvider.GetUserId()));
            }
            else
            {
                ArrestNameSnapshotBuilder.UpdateFromInput(snapshot, name.Id, name.RecordNumber, request.AtTimeOfName);
            }
        }
        else if (nameChanged || snapshot is null)
        {
            var snapshotLocations = await LoadSnapshotLocationsAsync(name, cancellationToken);
            if (snapshot is null)
            {
                _dbContext.ArrestNameSnapshots.Add(
                    ArrestNameSnapshotBuilder.CreateFromName(
                        arrest,
                        name,
                        snapshotLocations.PrimaryLocation,
                        snapshotLocations.SecondaryLocation,
                        _tenantProvider.GetUserId()));
            }
            else
            {
                snapshot.RefreshFromSource(
                    name.Id,
                    name.RecordNumber,
                    name.NameType,
                    name.LastOrBusinessName,
                    name.FirstName,
                    name.MiddleName,
                    name.SexId,
                    name.RaceId,
                    name.DateOfBirth,
                    name.DriversLicenseNumber,
                    name.DriversLicenseStateId,
                    name.HeightInches,
                    name.WeightLbs,
                    name.HairColorId,
                    name.EyeColorId,
                    name.SuffixId,
                    name.PlaceOfBirth,
                    name.FbiNumber,
                    name.LocalNumber,
                    name.PrimaryPhone,
                    name.PrimaryPhoneExtension,
                    name.WorkPhone,
                    name.WorkPhoneExtension,
                    name.OtherPhone,
                    name.OtherPhoneExtension,
                    name.SocialSecurityNumber,
                    name.IsCitizen,
                    name.DeceasedDate,
                    snapshotLocations.PrimaryLocation?.Id,
                    snapshotLocations.PrimaryLocation?.RecordNumber,
                    snapshotLocations.PrimaryLocation?.Address,
                    snapshotLocations.SecondaryLocation?.Id,
                    snapshotLocations.SecondaryLocation?.RecordNumber,
                    snapshotLocations.SecondaryLocation?.Address,
                    _tenantProvider.GetUserId());
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<(Location? PrimaryLocation, Location? SecondaryLocation)> LoadSnapshotLocationsAsync(
        Name name,
        CancellationToken cancellationToken)
    {
        var locationIds = new[] { name.PrimaryLocationId, name.SecondaryLocationId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (locationIds.Count == 0)
            return (null, null);

        var locations = await _dbContext.Locations
            .AsNoTracking()
            .Where(location => locationIds.Contains(location.Id))
            .ToDictionaryAsync(location => location.Id, cancellationToken);

        return (
            name.PrimaryLocationId.HasValue ? locations.GetValueOrDefault(name.PrimaryLocationId.Value) : null,
            name.SecondaryLocationId.HasValue ? locations.GetValueOrDefault(name.SecondaryLocationId.Value) : null);
    }
}
