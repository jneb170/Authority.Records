using Modules.Records.Application.DTOs;
using MediatR;

namespace Modules.Records.Application.Citations.Commands.SaveCitationPage;

public sealed record SaveCitationPageCommand(
    Guid CitationId,
    Guid? DefendantNameId,
    string Description,
    DateTime IssueDate,
    Guid? CourtId,
    string CitationNum,
    Guid? LocationId = null,
    NameSnapshotInput? AtTimeOfName = null,
    CitationOfficerProfileInput? OfficerProfile = null,
    CitationTexasDetailsInput? TexasDetails = null,
    CitationVehicleInput? Vehicle = null,
    IReadOnlyCollection<Guid>? IncidentIdsToAdd = null,
    IReadOnlyCollection<Guid>? IncidentIdsToRemove = null,
    IReadOnlyCollection<Guid>? ChargeIdsToAdd = null,
    IReadOnlyCollection<Guid>? ChargeIdsToRemove = null) : IRequest;
