using MediatR;

namespace Modules.Records.Application.Common.Queries.GetRecentActivity;

/// <summary>
/// Returns the most recently created or modified records for a given user,
/// across all four modules (Incidents, Arrests, Citations, Names). With
/// <see cref="IncludeAllUsers"/> it returns the most recent records in the
/// user's agency regardless of who touched them.
/// </summary>
public sealed record GetRecentActivityQuery(
    Guid UserId,
    Guid JurisdictionId,
    int  Take = 20,
    bool IncludeAllUsers = false) : IRequest<IReadOnlyList<RecentActivityDto>>;
