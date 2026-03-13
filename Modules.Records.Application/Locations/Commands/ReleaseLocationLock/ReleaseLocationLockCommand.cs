using MediatR;

namespace Modules.Records.Application.Locations.Commands.ReleaseLocationLock;

public sealed record ReleaseLocationLockCommand(Guid LocationId) : IRequest;
