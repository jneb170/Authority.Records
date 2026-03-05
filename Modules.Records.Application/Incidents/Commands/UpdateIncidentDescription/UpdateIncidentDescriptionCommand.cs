using MediatR;

namespace Modules.Records.Application.Incidents.Commands.UpdateIncidentDescription;

public sealed record UpdateIncidentDescriptionCommand(Guid IncidentId, string Description) : IRequest;
