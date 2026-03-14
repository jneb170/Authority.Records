using MediatR;

namespace Modules.Records.Application.Citations.Commands.UpdateCitationDetails;

public sealed record UpdateCitationDetailsCommand(
    Guid     CitationId,
    string   Description,
    DateTime IssueDate,
    Guid?    CourtId,
    string   CitationNum = "",
    Guid?    LocationId  = null) : IRequest;
