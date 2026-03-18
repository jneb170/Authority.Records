using MediatR;

namespace Modules.Records.Application.Charges.Commands.DeleteCharge;

public sealed record DeleteChargeCommand(Guid ChargeId) : IRequest;
