using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Names.Queries.GetNamesByJurisdiction;

public sealed record GetNamesByJurisdictionQuery : IRequest<IReadOnlyList<NameDto>>;
