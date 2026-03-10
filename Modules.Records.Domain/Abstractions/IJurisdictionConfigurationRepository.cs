using Modules.Records.Domain.Common;

namespace Modules.Records.Domain.Abstractions;

public interface IJurisdictionConfigurationRepository
{
    Task<JurisdictionConfiguration?> GetByJurisdictionIdAsync(
        Guid jurisdictionId,
        CancellationToken cancellationToken);

    Task SaveHotkeysAsync(
        Guid jurisdictionId,
        string? hotkeyNew,
        string? hotkeyModify,
        string? hotkeySave,
        string? hotkeyRelease,
        CancellationToken cancellationToken);
}
