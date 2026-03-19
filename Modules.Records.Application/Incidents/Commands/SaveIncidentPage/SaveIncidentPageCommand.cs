using MediatR;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Incidents.Commands.SaveIncidentPage;

public sealed record SaveIncidentPageCommand(
    Guid IncidentId,
    IncidentDetails Details,
    Guid? LocationId = null,
    DateTime? OccurredOn = null,
    IReadOnlyCollection<Guid>? ChargeIdsToAdd = null,
    IReadOnlyCollection<Guid>? ChargeIdsToRemove = null) : IRequest;
