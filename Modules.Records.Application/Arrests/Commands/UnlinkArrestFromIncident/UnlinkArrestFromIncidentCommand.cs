using MediatR;

namespace Modules.Records.Application.Arrests.Commands.UnlinkArrestFromIncident;

public sealed record UnlinkArrestFromIncidentCommand(
    Guid ArrestId,
    Guid IncidentId) : IRequest;
