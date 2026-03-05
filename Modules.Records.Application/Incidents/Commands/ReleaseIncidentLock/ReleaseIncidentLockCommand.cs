using MediatR;

namespace Modules.Records.Application.Incidents.Commands.ReleaseIncidentLock;

public sealed record ReleaseIncidentLockCommand(Guid IncidentId) : IRequest;
