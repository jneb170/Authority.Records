using MediatR;

namespace Modules.Records.Application.Arrests.Commands.SaveArrestPage;

public sealed record SaveArrestPageCommand(
    Guid ArrestId,
    Guid NameId,
    DateTime ArrestedAt,
    Guid? ArrestTypeId,
    string ArrestNum,
    Guid? LocationId = null,
    Guid? PrimaryIncidentId = null,
    IReadOnlyCollection<Guid>? IncidentIdsToAdd = null,
    IReadOnlyCollection<Guid>? IncidentIdsToRemove = null,
    IReadOnlyCollection<Guid>? ChargeIdsToAdd = null,
    IReadOnlyCollection<Guid>? ChargeIdsToRemove = null) : IRequest;
