using MediatR;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromArrest;

public sealed record UnlinkChargeFromArrestCommand(Guid ArrestId, Guid ChargeId) : IRequest;
