using MediatR;

namespace Modules.Records.Application.Charges.Commands.UpdateCharge;

public sealed record UpdateChargeCommand(
    Guid ChargeId,
    string OffenseName,
    string UcrCategory,
    string NibrsGroup,
    string CrimeAgainst,
    string UcrCode,
    string ChargeLevel,
    string? StateClass,
    bool IsCitationEligible,
    bool IsActive) : IRequest;
