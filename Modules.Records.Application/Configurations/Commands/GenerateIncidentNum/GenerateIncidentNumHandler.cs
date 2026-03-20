using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;

namespace Modules.Records.Application.Configurations.Commands.GenerateIncidentNum;

public sealed class GenerateIncidentNumHandler : IRequestHandler<GenerateIncidentNumCommand, string>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public GenerateIncidentNumHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<string> Handle(GenerateIncidentNumCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        if (agencyId == Guid.Empty)
            throw new InvalidOperationException("Select an active agency before generating an incident number.");
        var now = DateTime.UtcNow;
        var year = now.Year;

        var config = await _dbContext.AgencyConfigurations
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId && c.Key == ConfigurationKeys.IncidentFormat, cancellationToken);

        var formatTemplate = config?.Value ?? ConfigurationKeys.DefaultIncidentFormat;

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
                        c.CounterKey == ConfigurationKeys.IncidentFormat &&
                        c.Year == year, cancellationToken);
            }

            if (counter is null)
            {
                counter = new AgencySequenceCounter(jurisdictionId, agencyId, ConfigurationKeys.IncidentFormat, year);
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
                // Another user incremented the counter simultaneously — reload and retry
                await ex.Entries.Single().ReloadAsync(cancellationToken);
            }
            catch (DbUpdateException) when (isNew)
            {
                // Unique constraint — another user created the counter at the same time; detach and re-query
                _dbContext.Detach(counter);
                counter = null;
            }
        }

        throw new InvalidOperationException("Failed to generate an incident number after multiple attempts due to high concurrency. Please try again.");
    }
}
