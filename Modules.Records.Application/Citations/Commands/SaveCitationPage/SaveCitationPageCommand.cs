using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common.Violations;
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
    CitationOffenseDetailsInput? OffenseDetails = null,
    // The full set of manually-selected violation flags. Null leaves existing flags untouched;
    // non-null replaces the Manual flags with exactly this set (charge-derived flags are preserved).
    IReadOnlyCollection<ViolationFlagKey>? ViolationFlags = null,
    IReadOnlyCollection<Guid>? IncidentIdsToAdd = null,
    IReadOnlyCollection<Guid>? IncidentIdsToRemove = null,
    IReadOnlyCollection<Guid>? ChargeIdsToAdd = null,
    IReadOnlyCollection<Guid>? ChargeIdsToRemove = null) : IRequest;
