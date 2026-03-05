using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Incidents.Queries.GetIncidentsByJurisdiction;

public sealed record GetIncidentsByJurisdictionQuery(Guid JurisdictionId) : IRequest<IReadOnlyList<IncidentDto>>;
