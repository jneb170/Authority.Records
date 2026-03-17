using MediatR;

namespace Modules.Records.Application.Arrests.Commands.CreateArrest;

public sealed record CreateArrestCommand(
    Guid NameId,
    DateTime ArrestedAt,
    IReadOnlyList<long> IncidentRecordNumbers,
    string ArrestNum = "",
    Guid? PrimaryIncidentId = null) : IRequest<long>;
