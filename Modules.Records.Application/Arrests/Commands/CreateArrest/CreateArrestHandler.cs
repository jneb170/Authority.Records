using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
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

    public CreateArrestHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        ArrestFactory factory)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _factory = factory;
    }

    public async Task<long> Handle(CreateArrestCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var userId = _tenantProvider.GetUserId();

        var nameExists = await _dbContext.Names
            .AsNoTracking()
            .AnyAsync(n => n.Id == request.NameId && n.JurisdictionId == jurisdictionId, cancellationToken);

        if (!nameExists)
            throw new InvalidOperationException("Linked name not found.");

        long? primaryIncidentRecordNumber = null;
        if (request.PrimaryIncidentId.HasValue)
        {
            primaryIncidentRecordNumber = await _dbContext.IncidentReadModels
                .AsNoTracking()
                .Where(i => i.Id == request.PrimaryIncidentId.Value && i.JurisdictionId == jurisdictionId)
                .Select(i => (long?)i.RecordNumber)
                .FirstOrDefaultAsync(cancellationToken);

            if (!primaryIncidentRecordNumber.HasValue)
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

        _dbContext.Arrests.Add(arrest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Link to any specified incidents
        var incidentRecordNumbers = request.IncidentRecordNumbers
            .Concat(primaryIncidentRecordNumber.HasValue ? [primaryIncidentRecordNumber.Value] : [])
            .Distinct()
            .ToList();

        foreach (var recordNumber in incidentRecordNumbers)
        {
            var incident = await _dbContext.IncidentReadModels
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.RecordNumber == recordNumber, cancellationToken);

            if (incident is null) continue;

            var incidentEntity = await _dbContext.Incidents
                .FirstOrDefaultAsync(i => i.Id == incident.Id, cancellationToken);

            if (incidentEntity is null) continue;

            var link = new IncidentArrestLink(jurisdictionId, incidentEntity.Id, arrest.Id, userId);
            _dbContext.IncidentArrestLinks.Add(link);
        }

        if (incidentRecordNumbers.Any())
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
