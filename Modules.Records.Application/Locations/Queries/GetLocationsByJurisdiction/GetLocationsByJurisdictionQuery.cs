using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Locations.Queries.GetLocationsByJurisdiction;

public sealed record GetLocationsByJurisdictionQuery : IRequest<IReadOnlyList<LocationDto>>;
