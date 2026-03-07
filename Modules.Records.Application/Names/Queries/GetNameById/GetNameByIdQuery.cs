using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Names.Queries.GetNameById;

public sealed record GetNameByIdQuery(Guid Id) : IRequest<NameDto?>;
