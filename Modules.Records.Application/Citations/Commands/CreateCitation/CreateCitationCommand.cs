using MediatR;

namespace Modules.Records.Application.Citations.Commands.CreateCitation;

public sealed record CreateCitationCommand(
    string Description,
    DateTime IssueDate,
    IReadOnlyList<long> IncidentRecordNumbers,
    string CitationNum = "") : IRequest<long>;
