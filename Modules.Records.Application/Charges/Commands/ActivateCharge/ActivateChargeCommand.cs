using MediatR;

namespace Modules.Records.Application.Charges.Commands.ActivateCharge;

public sealed record ActivateChargeCommand(Guid ChargeId) : IRequest;
