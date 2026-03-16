using MediatR;

namespace Modules.Records.Application.Mugshots.Commands.UnlinkMugshotFromOwner;

public sealed record UnlinkMugshotFromOwnerCommand(
    Guid MugshotId,
    string OwnerType,
    Guid OwnerId) : IRequest;
