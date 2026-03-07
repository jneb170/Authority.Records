using MediatR;

namespace Modules.Records.Application.Citations.Commands.LinkCitationToIncident;

public sealed record LinkCitationToIncidentCommand(
    Guid CitationId,
    Guid IncidentId) : IRequest;
