using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Commands.CreateCitation;

public sealed class CreateCitationHandler : IRequestHandler<CreateCitationCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public CreateCitationHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<long> Handle(CreateCitationCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var userId = _tenantProvider.GetUserId();

        var citationNum = string.IsNullOrWhiteSpace(request.CitationNum)
            ? await TryGenerateCitationNumAsync(cancellationToken)
            : request.CitationNum;

        var citation = new Citation(
            jurisdictionId,
            agencyId,
            request.Description,
            request.IssueDate,
            citationNum);

        _dbContext.Citations.Add(citation);

        var incidentRecordNumbers = request.IncidentRecordNumbers
            .Distinct()
            .ToList();

        if (incidentRecordNumbers.Count > 0)
        {
            var incidentIds = await _dbContext.Incidents
                .AsNoTracking()
                .Where(i => i.JurisdictionId == jurisdictionId && incidentRecordNumbers.Contains(i.RecordNumber))
                .Select(i => i.Id)
                .ToListAsync(cancellationToken);

            foreach (var incidentId in incidentIds.Distinct())
            {
                _dbContext.IncidentCitationLinks.Add(new IncidentCitationLink(jurisdictionId, incidentId, citation.Id, userId));
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return citation.RecordNumber;
    }

    private async Task<string> TryGenerateCitationNumAsync(CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var now = DateTime.UtcNow;
        var year = now.Year;

        var config = await _dbContext.AgencyConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Key == ConfigurationKeys.CitationFormat, cancellationToken);

        var formatTemplate = config?.Value ?? ConfigurationKeys.DefaultCitationFormat;

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
                        c.CounterKey == ConfigurationKeys.CitationFormat &&
                        c.Year == year, cancellationToken);
            }

            if (counter is null)
            {
                counter = new AgencySequenceCounter(jurisdictionId, agencyId, ConfigurationKeys.CitationFormat, year);
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

        throw new InvalidOperationException("Could not generate a unique citation number after multiple attempts. Please try again.");
    }
}
