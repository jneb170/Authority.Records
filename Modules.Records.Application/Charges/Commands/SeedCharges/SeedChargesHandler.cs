using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Domain.Abstractions;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Charges.Commands.SeedCharges;

public sealed class SeedChargesHandler : IRequestHandler<SeedChargesCommand, ChargeSeedResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantProvider _tenantProvider;

    public SeedChargesHandler(IApplicationDbContext dbContext, ITenantProvider tenantProvider)
    {
        _dbContext = dbContext;
        _tenantProvider = tenantProvider;
    }

    public async Task<ChargeSeedResult> Handle(SeedChargesCommand request, CancellationToken cancellationToken)
    {
        var document = JsonSerializer.Deserialize<ChargeSeedDocument>(
            request.JsonContent,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            }) ?? throw new InvalidOperationException("Charge seed file is empty or invalid.");

        var jurisdictionId = _tenantProvider.GetJurisdictionId();
        var agencyId = _tenantProvider.GetAgencyId();
        var offenseNames = document.Offenses.Select(x => x.OffenseName.Trim()).Distinct().ToList();
        var codes = document.Offenses.Select(x => x.UcrCode.Trim()).Distinct().ToList();

        var existing = await _dbContext.Charges
            .Where(c => c.JurisdictionId == jurisdictionId &&
                        c.AgencyId == agencyId &&
                        offenseNames.Contains(c.OffenseName) &&
                        codes.Contains(c.UcrCode))
            .ToListAsync(cancellationToken);

        var inserted = 0;
        var updated = 0;

        foreach (var offense in document.Offenses)
        {
            var offenseName = offense.OffenseName.Trim();
            var ucrCategory = offense.UcrCategory.Trim();
            var nibrsGroup = offense.NibrsGroup.Trim();
            var crimeAgainst = offense.CrimeAgainst.Trim();
            var ucrCode = offense.UcrCode.Trim();
            var chargeLevel = offense.ChargeLevel.Trim();
            var stateClass = offense.StateClass?.Trim();

            var existingCharge = existing.FirstOrDefault(c =>
                c.OffenseName == offenseName &&
                c.UcrCode == ucrCode);

            if (existingCharge is null)
            {
                var charge = new Charge(
                    jurisdictionId,
                    agencyId,
                    offenseName,
                    ucrCategory,
                    nibrsGroup,
                    crimeAgainst,
                    ucrCode,
                    chargeLevel,
                    stateClass,
                    offense.IsCitationEligible);

                _dbContext.Charges.Add(charge);
                inserted++;
                continue;
            }

            existingCharge.Update(
                offenseName,
                ucrCategory,
                nibrsGroup,
                crimeAgainst,
                ucrCode,
                chargeLevel,
                stateClass,
                offense.IsCitationEligible);
            existingCharge.Activate();
            updated++;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new ChargeSeedResult(inserted, updated);
    }

    private sealed record ChargeSeedDocument(IReadOnlyList<ChargeSeedItem> Offenses);

    private sealed record ChargeSeedItem(
        string OffenseName,
        string UcrCategory,
        string NibrsGroup,
        string CrimeAgainst,
        string UcrCode,
        string ChargeLevel,
        string? StateClass,
        bool IsCitationEligible);
}
