using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Arrests;
using Modules.Records.Application.Common.Extensions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;

namespace Modules.Records.Application.Arrests.Commands.CreateArrest;

public sealed class CreateArrestHandler : IRequestHandler<CreateArrestCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly ArrestFactory _factory;
    private readonly IModificationContext _modificationContext;

    public CreateArrestHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        ArrestFactory factory,
        IModificationContext modificationContext)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _factory = factory;
        _modificationContext = modificationContext;
    }

    public async Task<long> Handle(CreateArrestCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var userId = _tenantProvider.GetUserId();

        if (agencyId == Guid.Empty)
            throw new InvalidOperationException("Select an active agency before creating an arrest.");

        var name = await _dbContext.Names
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == request.NameId && n.JurisdictionId == jurisdictionId, cancellationToken)
            ?? throw new InvalidOperationException("Linked name not found.");

        if (request.LocationId.HasValue)
        {
            var locationExists = await _dbContext.Locations
                .AsNoTracking()
                .AnyAsync(l => l.Id == request.LocationId.Value && l.JurisdictionId == jurisdictionId, cancellationToken);

            if (!locationExists)
                throw new InvalidOperationException("Linked location not found.");
        }

        Guid? primaryIncidentId = null;
        if (request.PrimaryIncidentId.HasValue)
        {
            primaryIncidentId = await _dbContext.Incidents
                .AsNoTracking()
                .WhereAgencyScoped(agencyId)
                .Where(i => i.Id == request.PrimaryIncidentId.Value && i.JurisdictionId == jurisdictionId)
                .Select(i => (Guid?)i.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (!primaryIncidentId.HasValue)
                throw new InvalidOperationException("Primary incident not found.");
        }

        var arrestNum = string.IsNullOrWhiteSpace(request.ArrestNum)
            ? await TryGenerateArrestNumAsync(cancellationToken)
            : request.ArrestNum;

        var arrest = _factory.Create(
            jurisdictionId,
            agencyId,
            request.NameId,
            request.ArrestedAt,
            arrestNum,
            request.PrimaryIncidentId);

        arrest.SetLocation(request.LocationId, _modificationContext);

        _dbContext.Arrests.Add(arrest);

        var snapshotLocations = await ArrestNameSnapshotBuilder.LoadSnapshotLocationsAsync(_dbContext, name, cancellationToken);
        _dbContext.ArrestNameSnapshots.Add(
            ArrestNameSnapshotBuilder.CreateFromName(
                arrest,
                name,
                snapshotLocations.PrimaryLocation,
                snapshotLocations.SecondaryLocation,
                userId));

        var incidentRecordNumbers = request.IncidentRecordNumbers
            .Distinct()
            .ToList();

        var linkedIncidentIds = new HashSet<Guid>();

        if (incidentRecordNumbers.Count > 0)
        {
            var incidentIds = await _dbContext.Incidents
                .AsNoTracking()
                .WhereAgencyScoped(agencyId)
                .Where(i => i.JurisdictionId == jurisdictionId && incidentRecordNumbers.Contains(i.RecordNumber))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            linkedIncidentIds.UnionWith(incidentIds);
        }

        if (primaryIncidentId.HasValue)
            linkedIncidentIds.Add(primaryIncidentId.Value);

        foreach (var incidentId in linkedIncidentIds)
        {
            _dbContext.IncidentArrestLinks.Add(new IncidentArrestLink(jurisdictionId, incidentId, arrest.Id, userId));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return arrest.RecordNumber;
    }

    private async Task<string> TryGenerateArrestNumAsync(CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var now = DateTime.UtcNow;
        var year = now.Year;

        var config = await _dbContext.AgencyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Key == ConfigurationKeys.ArrestFormat, cancellationToken);

        var formatTemplate = config?.Value ?? ConfigurationKeys.DefaultArrestFormat;

        AgencySequenceCounter? counter = null;

        const int maxRetries = 5;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            bool isNew = false;

            if (counter is null)
            {
                counter = await _dbContext.AgencySequenceCounters
                    .FirstOrDefaultAsync(c =>
                        c.JurisdictionId == jurisdictionId &&
                        c.AgencyId == agencyId &&
                        c.CounterKey == ConfigurationKeys.ArrestFormat &&
                        c.Year == year, cancellationToken);
            }

            if (counter is null)
            {
                counter = new AgencySequenceCounter(jurisdictionId, agencyId, ConfigurationKeys.ArrestFormat, year);
                _dbContext.AgencySequenceCounters.Add(counter);
                isNew = true;
            }

            var seq = counter.Increment();

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return IncidentNumFormatter.Format(formatTemplate, year, now.Month, now.Day, seq);
            }
            catch (DbUpdateConcurrencyException ex) when (!isNew)
            {
                await ex.Entries.Single().ReloadAsync(cancellationToken);
            }
            catch (DbUpdateException) when (isNew)
            {
                _dbContext.Detach(counter);
                counter = null;
            }
        }

        throw new InvalidOperationException("Could not generate a unique arrest number after multiple attempts. Please try again.");
    }
}
