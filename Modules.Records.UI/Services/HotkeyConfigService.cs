using Modules.Records.Domain.Abstractions;

namespace Modules.Records.UI.Services;

public sealed class HotkeyConfigService : IHotkeyConfigService
{
    private readonly IJurisdictionConfigurationRepository _repo;
    private readonly ITenantProvider _tenantProvider;

    // Lazy<Task<T>> ensures only ONE DB call is ever made, even if multiple
    // components call GetBindingsAsync() concurrently before the first completes.
    private Lazy<Task<HotkeyBindings>>? _lazy;

    public HotkeyConfigService(
        IJurisdictionConfigurationRepository repo,
        ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public Task<HotkeyBindings> GetBindingsAsync()
    {
        _lazy ??= new Lazy<Task<HotkeyBindings>>(LoadAsync);
        return _lazy.Value;
    }

    private async Task<HotkeyBindings> LoadAsync()
    {
        try
        {
            var jurisdictionId = _tenantProvider.GetJurisdictionId();
            if (jurisdictionId == Guid.Empty)
                return HotkeyBindings.Default;

            var config = await _repo.GetByJurisdictionIdAsync(jurisdictionId, CancellationToken.None);

            return new HotkeyBindings(
                New:     config?.HotkeyNew     ?? HotkeyBindings.Default.New,
                Modify:  config?.HotkeyModify  ?? HotkeyBindings.Default.Modify,
                Save:    config?.HotkeySave    ?? HotkeyBindings.Default.Save,
                Release: config?.HotkeyRelease ?? HotkeyBindings.Default.Release);
        }
        catch
        {
            return HotkeyBindings.Default;
        }
    }

    public void InvalidateCache() => _lazy = null;
}
