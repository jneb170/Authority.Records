using Modules.Records.Domain.Abstractions;
using Shared.Infrastructure.Persistence;

namespace Modules.Records.Infrastructure.Services;

public sealed class JurisdictionRulesService : IJurisdictionRulesService
{
    private readonly AppDbContext _context;

    public JurisdictionRulesService(AppDbContext context)
    {
        _context = context;
    }

    public bool MustCloseAllArrests(Guid jurisdictionId)
    {
        var config = _context.JurisdictionConfigurations
            .FirstOrDefault(x => x.JurisdictionId == jurisdictionId);

        return config?.MustCloseAllArrests ?? false;
    }

    public bool MustCloseAllCitations(Guid jurisdictionId)
    {
        var config = _context.JurisdictionConfigurations
            .FirstOrDefault(x => x.JurisdictionId == jurisdictionId);

        return config?.MustCloseAllCitations ?? false;
    }
}