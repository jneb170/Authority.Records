using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Common.Extensions;

internal static class AgencyScopeExtensions
{
    public static IQueryable<NameReadModel> WhereAgencyScoped(this IQueryable<NameReadModel> query, Guid agencyId)
        => agencyId == Guid.Empty ? query.Where(_ => false) : query.Where(x => x.AgencyId == agencyId);

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
