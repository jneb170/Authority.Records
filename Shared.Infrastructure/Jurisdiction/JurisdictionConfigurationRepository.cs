using Microsoft.EntityFrameworkCore;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Common;
using Shared.Infrastructure.Persistence;


namespace Shared.Infrastructure.Jurisdiction;

public sealed class JurisdictionConfigurationRepository
    : IJurisdictionConfigurationRepository
{
    private readonly AppDbContext _context;

    public JurisdictionConfigurationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<JurisdictionConfiguration?> GetByJurisdictionIdAsync(
        Guid jurisdictionId,
        CancellationToken cancellationToken)
    {
        return await _context.JurisdictionConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.JurisdictionId == jurisdictionId, cancellationToken);
    }

    public async Task SaveHotkeysAsync(
        Guid jurisdictionId,
        string? hotkeyNew,
        string? hotkeyModify,
        string? hotkeySave,
        string? hotkeyRelease,
        CancellationToken cancellationToken)
    {
        var entity = await _context.JurisdictionConfigurations
            .FirstOrDefaultAsync(x => x.JurisdictionId == jurisdictionId, cancellationToken);

        if (entity is null)
        {
            entity = new JurisdictionConfiguration(jurisdictionId, false, false);
            _context.JurisdictionConfigurations.Add(entity);
        }

        entity.UpdateHotkeys(hotkeyNew, hotkeyModify, hotkeySave, hotkeyRelease);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveGoogleMapsApiKeyAsync(
        Guid jurisdictionId,
        string? apiKey,
        CancellationToken cancellationToken)
    {
        var entity = await _context.JurisdictionConfigurations
            .FirstOrDefaultAsync(x => x.JurisdictionId == jurisdictionId, cancellationToken);

        if (entity is null)
        {
            entity = new JurisdictionConfiguration(jurisdictionId, false, false);
            _context.JurisdictionConfigurations.Add(entity);
        }

        entity.UpdateGoogleMapsApiKey(apiKey);
        await _context.SaveChangesAsync(cancellationToken);
    }
}