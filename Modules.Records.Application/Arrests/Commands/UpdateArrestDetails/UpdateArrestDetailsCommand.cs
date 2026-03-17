using MediatR;

namespace Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;

public sealed record UpdateArrestDetailsCommand(
    Guid     ArrestId,
    Guid     NameId,
    DateTime ArrestedAt,
    Guid?    ArrestTypeId,
    string   ArrestNum   = "",
    Guid?    LocationId  = null,
    Guid?    PrimaryIncidentId = null) : IRequest;
