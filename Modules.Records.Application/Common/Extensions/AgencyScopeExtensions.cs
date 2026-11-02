using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Common.Extensions;

/// <summary>
/// Limits agency-owned read models (incidents, arrests, citations) to one agency. Names and
/// locations are a master index shared by every agency in the jurisdiction, so they have no
/// agency scope (issue #118). The aggregates themselves are also covered by the Agency global
/// query filter on AppDbContext.
/// </summary>
internal static class AgencyScopeExtensions
{
    public static IQueryable<IncidentReadModel> WhereAgencyScoped(this IQueryable<IncidentReadModel> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

    public static IQueryable<ArrestReadModel> WhereAgencyScoped(this IQueryable<ArrestReadModel> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

    public static IQueryable<CitationReadModel> WhereAgencyScoped(this IQueryable<CitationReadModel> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

    public static IQueryable<Incident> WhereAgencyScoped(this IQueryable<Incident> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

    public static IQueryable<Arrest> WhereAgencyScoped(this IQueryable<Arrest> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

    public static IQueryable<Citation> WhereAgencyScoped(this IQueryable<Citation> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);
}
