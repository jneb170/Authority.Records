using MediatR;
using Modules.Records.Domain.Abstractions;

namespace Modules.Records.Application.Configurations.Commands.UpdateJurisdictionHotkeys;

public sealed class UpdateJurisdictionHotkeysHandler : IRequestHandler<UpdateJurisdictionHotkeysCommand>
{
    private readonly IJurisdictionConfigurationRepository _repo;
    private readonly ITenantProvider _tenantProvider;

    public UpdateJurisdictionHotkeysHandler(
        IJurisdictionConfigurationRepository repo,
        ITenantProvider tenantProvider)
    {
        _repo = repo;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(UpdateJurisdictionHotkeysCommand request, CancellationToken cancellationToken)
    {
        var jurisdictionId = _tenantProvider.GetJurisdictionId();

        await _repo.SaveHotkeysAsync(
            jurisdictionId,
            request.HotkeyNew,
            request.HotkeyModify,
            request.HotkeySave,
            request.HotkeyRelease,
            cancellationToken);
    }
}
