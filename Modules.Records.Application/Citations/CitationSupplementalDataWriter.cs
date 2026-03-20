using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.DTOs;
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
                input.PageNumber,
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
            input.DocketNumber,
            input.PageNumber,
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
}
