using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
using Modules.Records.Domain.Common.Violations;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations;

internal static class CitationSupplementalDataWriter
{
    public static async Task ApplyOfficerProfileAsync(
        IApplicationDbContext dbContext,
        Citation citation,
        CitationOfficerProfileInput? input,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.CitationOfficerProfiles
            .FirstOrDefaultAsync(profile => profile.CitationId == citation.Id, cancellationToken);

        if (input is null)
        {
            if (existing is not null)
                dbContext.CitationOfficerProfiles.Remove(existing);

            return;
        }

        if (existing is null)
        {
            dbContext.CitationOfficerProfiles.Add(new CitationOfficerProfile(
                citation.JurisdictionId,
                citation.AgencyId,
                citation.Id,
                input.SourceNameId,
                input.SourceNameRecordNumber,
                input.OfficerName,
                input.Title,
                input.BadgeOrIdentifier,
                input.UnitNumber));
            return;
        }

        existing.UpdateDetails(
            input.SourceNameId,
            input.SourceNameRecordNumber,
            input.OfficerName,
            input.Title,
            input.BadgeOrIdentifier,
            input.UnitNumber);
    }

    public static async Task ApplyVehicleAsync(
        IApplicationDbContext dbContext,
        Citation citation,
        CitationVehicleInput? input,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.CitationVehicles
            .FirstOrDefaultAsync(vehicle => vehicle.CitationId == citation.Id, cancellationToken);

        if (input is null)
        {
            if (existing is not null)
                dbContext.CitationVehicles.Remove(existing);

            return;
        }

        if (existing is null)
        {
            dbContext.CitationVehicles.Add(new CitationVehicle(
                citation.JurisdictionId,
                citation.AgencyId,
                citation.Id,
                input.PlateNumber,
                input.PlateStateId,
                input.PlateYear,
                input.ModelYear,
                input.Make,
                input.Style,
                input.Color,
                input.IsCommercial,
                input.CarriesHazardousMaterial));
            return;
        }

        existing.UpdateDetails(
            input.PlateNumber,
            input.PlateStateId,
            input.PlateYear,
            input.ModelYear,
            input.Make,
            input.Style,
            input.Color,
            input.IsCommercial,
            input.CarriesHazardousMaterial);
    }

    public static async Task ApplyTexasDetailsAsync(
        IApplicationDbContext dbContext,
        Citation citation,
        CitationTexasDetailsInput? input,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.CitationTexasDetails
            .FirstOrDefaultAsync(details => details.CitationId == citation.Id, cancellationToken);

        if (input is null)
        {
            if (existing is not null)
                dbContext.CitationTexasDetails.Remove(existing);

            return;
        }

        if (existing is null)
        {
            dbContext.CitationTexasDetails.Add(new CitationTexasDetails(
                citation.JurisdictionId,
                citation.AgencyId,
                citation.Id,
                input.DocketNumber,
                input.PageNumber));
            return;
        }

        existing.UpdateDetails(
            input.DocketNumber,
            input.PageNumber);
    }

    public static async Task ApplyOffenseDetailsAsync(
        IApplicationDbContext dbContext,
        Citation citation,
        CitationOffenseDetailsInput? input,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.CitationOffenseDetails
            .FirstOrDefaultAsync(details => details.CitationId == citation.Id, cancellationToken);

        if (input is null)
        {
            if (existing is not null)
                dbContext.CitationOffenseDetails.Remove(existing);

            return;
        }

        if (existing is null)
        {
            dbContext.CitationOffenseDetails.Add(new CitationOffenseDetails(
                citation.JurisdictionId,
                citation.AgencyId,
                citation.Id,
                input.ViolationSourceTypeId,
                input.ViolationSection,
                input.ViolationGroupId,
                input.PrimaryViolationDescription,
                input.SpeedMph,
                input.ZoneMph,
                input.SpeedBandId,
                input.NarrativeOtherViolations,
                input.OccurredAtText,
                input.CourtAppearanceDateTime,
                input.CourtAppearanceLocationId,
                input.AffidavitSignedDate,
                input.ComplainantSignatureText,
                input.DefendantSignatureText,
                input.AcceptedBondNotes,
                input.ReceiptNumber));
            return;
        }

        existing.UpdateDetails(
            input.ViolationSourceTypeId,
            input.ViolationSection,
            input.ViolationGroupId,
            input.PrimaryViolationDescription,
            input.SpeedMph,
            input.ZoneMph,
            input.SpeedBandId,
            input.NarrativeOtherViolations,
            input.OccurredAtText,
            input.CourtAppearanceDateTime,
            input.CourtAppearanceLocationId,
            input.AffidavitSignedDate,
            input.ComplainantSignatureText,
            input.DefendantSignatureText,
            input.AcceptedBondNotes,
            input.ReceiptNumber);
    }

    /// <summary>
    /// Reconciles the Manual violation flags for a citation to exactly <paramref name="desiredKeys"/>.
    /// A null set leaves all flags untouched. Charge-derived flags (Source != Manual) are never
    /// added or removed here — they are owned by the (future) charge-derivation path.
    /// </summary>
    public static async Task ApplyViolationFlagsAsync(
        IApplicationDbContext dbContext,
        Citation citation,
        IReadOnlyCollection<ViolationFlagKey>? desiredKeys,
        CancellationToken cancellationToken)
    {
        if (desiredKeys is null)
            return;

        var desired = desiredKeys.Distinct().ToHashSet();

        var existing = await dbContext.CitationViolationFlags
            .Where(flag => flag.CitationId == citation.Id)
            .ToListAsync(cancellationToken);

        var existingManual = existing.Where(flag => flag.Source == ViolationFlagSource.Manual).ToList();
        var existingManualKeys = existingManual.Select(flag => flag.Key).ToHashSet();

        // Remove manual flags the officer unticked.
        var toRemove = existingManual.Where(flag => !desired.Contains(flag.Key)).ToList();
        if (toRemove.Count > 0)
            dbContext.CitationViolationFlags.RemoveRange(toRemove);

        // Add manual flags the officer newly ticked (skip any already present from any source).
        var existingAnyKeys = existing.Select(flag => flag.Key).ToHashSet();
        foreach (var key in desired.Where(key => !existingManualKeys.Contains(key) && !existingAnyKeys.Contains(key)))
        {
            dbContext.CitationViolationFlags.Add(new CitationViolationFlag(
                citation.JurisdictionId,
                citation.AgencyId,
                citation.Id,
                key,
                ViolationFlagSource.Manual));
        }
    }
}
