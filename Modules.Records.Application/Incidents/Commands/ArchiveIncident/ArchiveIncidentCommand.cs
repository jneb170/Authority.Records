using MediatR;

namespace Modules.Records.Application.Incidents.Commands.ArchiveIncident;

public sealed record ArchiveIncidentCommand(Guid IncidentId) : IRequest;
