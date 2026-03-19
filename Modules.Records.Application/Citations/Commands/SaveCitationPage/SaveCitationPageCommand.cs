using MediatR;

namespace Modules.Records.Application.Citations.Commands.SaveCitationPage;

public sealed record SaveCitationPageCommand(
    Guid CitationId,
    string Description,
    DateTime IssueDate,
    Guid? CourtId,
    string CitationNum,
    Guid? LocationId = null,
    IReadOnlyCollection<Guid>? IncidentIdsToAdd = null,
    IReadOnlyCollection<Guid>? IncidentIdsToRemove = null,
    IReadOnlyCollection<Guid>? ChargeIdsToAdd = null,
    IReadOnlyCollection<Guid>? ChargeIdsToRemove = null) : IRequest;
