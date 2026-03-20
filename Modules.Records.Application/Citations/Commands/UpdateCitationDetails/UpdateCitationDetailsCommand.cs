using Modules.Records.Application.DTOs;
using MediatR;

namespace Modules.Records.Application.Citations.Commands.UpdateCitationDetails;

public sealed record UpdateCitationDetailsCommand(
    Guid     CitationId,
    Guid?    DefendantNameId,
    string   Description,
    DateTime IssueDate,
    Guid?    CourtId,
    string   CitationNum = "",
    Guid?    LocationId  = null,
    NameSnapshotInput? AtTimeOfName = null,
    CitationOfficerProfileInput? OfficerProfile = null,
    CitationTexasDetailsInput? TexasDetails = null,
    CitationVehicleInput? Vehicle = null) : IRequest;
