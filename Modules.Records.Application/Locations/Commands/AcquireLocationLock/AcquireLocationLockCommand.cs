using MediatR;

namespace Modules.Records.Application.Locations.Commands.AcquireLocationLock;

public sealed record AcquireLocationLockCommand(Guid LocationId) : IRequest;
