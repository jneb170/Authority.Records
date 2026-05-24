using MediatR;
using Modules.Records.Application.Common;

namespace Modules.Records.Application.Citations.Commands.CreateCitation;

public sealed record CreateCitationCommand(
    string Description,
    DateTime IssueDate,
    IReadOnlyList<long> IncidentRecordNumbers,
    string CitationNum = "") : IRequest<long>, IRateLimitedCommand;
