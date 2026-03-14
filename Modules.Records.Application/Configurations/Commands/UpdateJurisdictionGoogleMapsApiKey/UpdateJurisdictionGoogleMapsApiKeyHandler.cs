using MediatR;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Configurations.Commands.UpdateJurisdictionGoogleMapsApiKey;

public sealed class UpdateJurisdictionGoogleMapsApiKeyHandler
    : IRequestHandler<UpdateJurisdictionGoogleMapsApiKeyCommand>
{
    private readonly IJurisdictionConfigurationRepository _repo;
    private readonly ITenantProvider _tenantProvider;

    public UpdateJurisdictionGoogleMapsApiKeyHandler(
        IJurisdictionConfigurationRepository repo,
        ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(
        UpdateJurisdictionGoogleMapsApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        await _repo.SaveGoogleMapsApiKeyAsync(
            jurisdictionId,
            request.ApiKey,
            cancellationToken);
    }
}
