using MediatR;

namespace Modules.Records.Application.Arrests.Commands.CreateArrest;

public sealed record CreateArrestCommand(
    string SuspectName,
    DateTime ArrestedAt,
    IReadOnlyList<long> IncidentRecordNumbers,
    string ArrestNum = "") : IRequest<long>;
