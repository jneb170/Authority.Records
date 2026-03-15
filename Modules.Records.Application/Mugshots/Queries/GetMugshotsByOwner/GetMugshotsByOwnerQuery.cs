using MediatR;
using Modules.Records.Application.DTOs;

namespace Modules.Records.Application.Mugshots.Queries.GetMugshotsByOwner;

public sealed record GetMugshotsByOwnerQuery(
    string OwnerType,
    Guid OwnerId) : IRequest<IReadOnlyList<MugshotDto>>;
