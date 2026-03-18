using MediatR;

namespace Modules.Records.Application.Charges.Commands.LinkChargeToCitation;

public sealed record LinkChargeToCitationCommand(Guid CitationId, Guid ChargeId) : IRequest;
