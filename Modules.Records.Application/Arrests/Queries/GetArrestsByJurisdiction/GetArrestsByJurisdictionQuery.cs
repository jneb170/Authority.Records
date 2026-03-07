using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestsByJurisdiction;

public sealed record GetArrestsByJurisdictionQuery : IRequest<IReadOnlyList<ArrestDto>>;
