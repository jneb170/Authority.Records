using MediatR;

namespace Modules.Records.Application.Arrests.Commands.ReleaseArrestLock;

public sealed record ReleaseArrestLockCommand(Guid ArrestId) : IRequest;
