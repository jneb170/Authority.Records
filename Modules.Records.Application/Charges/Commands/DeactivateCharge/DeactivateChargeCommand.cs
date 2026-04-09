using MediatR;

namespace Modules.Records.Application.Charges.Commands.DeactivateCharge;

public sealed record DeactivateChargeCommand(Guid ChargeId) : IRequest;
