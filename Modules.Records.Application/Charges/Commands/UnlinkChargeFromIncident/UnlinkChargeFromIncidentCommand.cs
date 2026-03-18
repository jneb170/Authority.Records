using MediatR;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromIncident;

public sealed record UnlinkChargeFromIncidentCommand(Guid IncidentId, Guid ChargeId) : IRequest;
