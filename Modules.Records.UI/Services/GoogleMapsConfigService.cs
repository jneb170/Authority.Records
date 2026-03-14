using Modules.Records.Domain.Abstractions;

namespace Modules.Records.UI.Services;

public sealed class GoogleMapsConfigService : IGoogleMapsConfigService
{
    private readonly IJurisdictionConfigurationRepository _repo;
    private readonly ITenantProvider _tenantProvider;

    private Lazy<Task<string?>>? _lazy;

    public GoogleMapsConfigService(
        IJurisdictionConfigurationRepository repo,
        ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public Task<string?> GetApiKeyAsync()
    {
        _lazy ??= new Lazy<Task<string?>>(LoadAsync);
        return _lazy.Value;
    }

    private async Task<string?> LoadAsync()
    {
        try
        {
            var jurisdictionId = _tenantProvider.GetJurisdictionId();
            if (jurisdictionId == Guid.Empty)
                return null;

            var config = await _repo.GetByJurisdictionIdAsync(jurisdictionId, CancellationToken.None);
            return config?.GoogleMapsApiKey;
        }
        catch
        {
            return null;
        }
    }

    public void InvalidateCache() => _lazy = null;
}
