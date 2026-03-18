using MediatR;

namespace Modules.Records.Application.Charges.Commands.CreateCharge;

public sealed record CreateChargeCommand(
    string OffenseName,
    string UcrCategory,
    string NibrsGroup,
    string CrimeAgainst,
    string UcrCode,
    string ChargeLevel,
    string? StateClass,
    bool IsCitationEligible,
    bool IsActive = true) : IRequest<Guid>;
