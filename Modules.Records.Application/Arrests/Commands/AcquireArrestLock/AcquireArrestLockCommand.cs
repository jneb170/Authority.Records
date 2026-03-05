using MediatR;

namespace Modules.Records.Application.Arrests.Commands.AcquireArrestLock;

public sealed record AcquireArrestLockCommand(Guid ArrestId) : IRequest;
