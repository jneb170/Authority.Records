using MediatR;

namespace Modules.Records.Application.Charges.Commands.SeedCharges;

public sealed record SeedChargesCommand(string JsonContent) : IRequest<ChargeSeedResult>;
