using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Incidents.Commands.CreateIncident;

public sealed class CreateIncidentHandler : IRequestHandler<CreateIncidentCommand, long>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;
    private readonly IncidentFactory _factory;

    public CreateIncidentHandler(
        IApplicationDbContext dbContext,
        ITenantProvider tenantProvider,
        IncidentFactory factory)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
        _factory = factory;
    }

    public async Task<long> Handle(CreateIncidentCommand request, CancellationToken cancellationToken)
    {
        var agencyId = _tenantProvider.GetAgencyId();
        if (agencyId == Guid.Empty)
            throw new InvalidOperationException("Select an active agency before creating an incident.");

        var details = request.Details;

        // Auto-generate IncidentNum if the user left it blank (uses agency config or system default)
        if (string.IsNullOrWhiteSpace(details.IncidentNum))
        {
            var generated = await TryGenerateIncidentNumAsync(cancellationToken);
            details = details with { IncidentNum = generated };
        }

        var incident = _factory.Create(new CreateIncidentRequest
        {
            JurisdictionId = _tenantProvider.GetJurisdictionId(),
            AgencyId       = agencyId,
            Details        = details,
        });

        _dbContext.Incidents.Add(incident);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return incident.RecordNumber;
    }

    /// <summary>
    /// Atomically reserves the next sequence number using the agency's IncidentFormat configuration.
    /// Falls back to the system default format when no agency-specific format is configured.
    /// Always returns a value — never null.
    /// </summary>
    private async Task<string> TryGenerateIncidentNumAsync(CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var now = DateTime.UtcNow;
        var year = now.Year;

        var config = await _dbContext.AgencyConfigurations
            .AsNoTracking()
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
                await ex.Entries.Single().ReloadAsync(cancellationToken);
            }
            catch (DbUpdateException) when (isNew)
            {
                _dbContext.Detach(counter);
                counter = null;
            }
        }

        throw new InvalidOperationException("Could not generate a unique incident number after multiple attempts. Please try again.");
    }
}

