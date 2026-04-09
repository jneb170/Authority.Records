using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges;

internal static class ChargeMappings
{
    public static ChargeDto ToDto(this Charge charge) => new(
        charge.Id,
        charge.JurisdictionId,
        charge.AgencyId,
        charge.OffenseName,
        charge.UcrCategory,
        charge.NibrsGroup,
        charge.CrimeAgainst,
        charge.UcrCode,
        charge.ChargeLevel,
        charge.StateClass,
        charge.IsCitationEligible,
        charge.IsActive);
}
