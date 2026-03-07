using MediatR;

namespace Modules.Records.Application.Arrests.Commands.LinkArrestToIncident;

public sealed record LinkArrestToIncidentCommand(
    Guid ArrestId,
    Guid IncidentId) : IRequest;
