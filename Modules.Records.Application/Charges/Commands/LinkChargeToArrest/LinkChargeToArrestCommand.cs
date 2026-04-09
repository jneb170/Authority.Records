using MediatR;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToArrest;

public sealed record LinkChargeToArrestCommand(Guid ArrestId, Guid ChargeId) : IRequest;
