using MediatR;

namespace Modules.Records.Application.Mugshots.Commands.SetPrimaryMugshot;

public sealed record SetPrimaryMugshotCommand(
    Guid MugshotId,
    string OwnerType,
    Guid OwnerId) : IRequest;
