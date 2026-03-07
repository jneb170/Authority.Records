using MediatR;

namespace Modules.Records.Application.Citations.Commands.UnlinkCitationFromIncident;

public sealed record UnlinkCitationFromIncidentCommand(
    Guid CitationId,
    Guid IncidentId) : IRequest;
