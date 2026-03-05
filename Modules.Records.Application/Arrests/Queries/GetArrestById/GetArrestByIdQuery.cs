using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Arrests.Queries.GetArrestById;

public sealed record GetArrestByIdQuery(Guid ArrestId) : IRequest<ArrestDto?>;
