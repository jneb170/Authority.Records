using MediatR;

namespace Modules.Records.Application.Common.Queries.GetRecentActivity;

/// <summary>
/// Returns the most recently created or modified records for a given user,
/// across all four modules (Incidents, Arrests, Citations, Names).
/// </summary>
public sealed record GetRecentActivityQuery(
    Guid UserId,
    Guid JurisdictionId,
    int  Take = 20) : IRequest<IReadOnlyList<RecentActivityDto>>;
