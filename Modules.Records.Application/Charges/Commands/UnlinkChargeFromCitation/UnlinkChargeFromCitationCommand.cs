using MediatR;

namespace Modules.Records.Application.Charges.Commands.UnlinkChargeFromCitation;

public sealed record UnlinkChargeFromCitationCommand(Guid CitationId, Guid ChargeId) : IRequest;
