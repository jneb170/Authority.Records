using MediatR;

namespace Modules.Records.Application.Arrests.Commands.UpdateArrestDetails;

public sealed record UpdateArrestDetailsCommand(
    Guid     ArrestId,
    string   SuspectName,
    DateTime ArrestedAt,
    Guid?    ArrestTypeId,
    string   ArrestNum   = "",
    Guid?    LocationId  = null) : IRequest;
