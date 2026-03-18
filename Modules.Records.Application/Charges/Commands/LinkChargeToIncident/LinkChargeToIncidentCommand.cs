using MediatR;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToIncident;

public sealed record LinkChargeToIncidentCommand(Guid IncidentId, Guid ChargeId) : IRequest;
