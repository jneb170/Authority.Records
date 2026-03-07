using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetIncidentsByArrest;

public sealed record GetIncidentsByArrestQuery(Guid ArrestId) : IRequest<IReadOnlyList<IncidentArrestLinkDto>>;
