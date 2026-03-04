using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Exceptions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Domain.Services;

public sealed class IncidentCloseDomainService
{
    private readonly IArrestRepository _arrestRepository;
    private readonly IJurisdictionConfigurationRepository _configRepository;

    public IncidentCloseDomainService(
        IArrestRepository arrestRepository,
        IJurisdictionConfigurationRepository configRepository)
    {
        _arrestRepository = arrestRepository
            ?? throw new ArgumentNullException(nameof(arrestRepository));

        _configRepository = configRepository
            ?? throw new ArgumentNullException(nameof(configRepository));
    }

    public async Task ValidateCanCloseAsync(
        Incident incident,
        bool isForced,
        CancellationToken cancellationToken)
    {
        if (isForced)
            return;

        var config = await _configRepository
            .GetByJurisdictionIdAsync(
                incident.JurisdictionId,
                cancellationToken);

        if (config is null)
            return; // default behavior if not configured

        if (!config.MustCloseArrestsBeforeIncidentClose)
            return;

        var arrests = await _arrestRepository
            .GetByIncidentIdAsync(
                incident.Id,
                cancellationToken);

        var openArrests = arrests
            .Where(a => a.Status != RecordStatus.Closed &&
                        a.Status != RecordStatus.Archived)
            .ToList();

        if (openArrests.Any())
        {
            throw new DomainException(
                "incident.close.arrests.open",
                $"Incident cannot be closed. {openArrests.Count} arrest(s) are not closed.");
        }
    }
}