using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Locations.Queries.GetLocationById;

public sealed record GetLocationByIdQuery(Guid Id) : IRequest<LocationDto?>;
