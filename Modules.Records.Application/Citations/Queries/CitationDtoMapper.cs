using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Abstractions;
using Modules.Records.Application.Citations;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.ReadModels;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Entities;

namespace Modules.Records.Application.Citations.Queries;

internal static class CitationDtoMapper
{
    public static async Task<IReadOnlyList<CitationDto>> ToDtosAsync(
        IReadOnlyList<CitationReadModel> citations,
        IApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        if (citations.Count == 0)
            return [];

        var defendantNameIds = citations
            .Where(c => c.DefendantNameId.HasValue)
            .Select(c => c.DefendantNameId!.Value)
            .Distinct()
            .ToList();

        var citationIds = citations
            .Select(c => c.Id)
            .Distinct()
            .ToList();

        var names = defendantNameIds.Count > 0
            ? await dbContext.NameReadModels
                .AsNoTracking()
                .Where(n => defendantNameIds.Contains(n.Id))
                .ToDictionaryAsync(n => n.Id, cancellationToken)
            : new Dictionary<Guid, NameReadModel>();

        var snapshots = citationIds.Count > 0
            ? await dbContext.CitationNameSnapshots
                .AsNoTracking()
                .Where(snapshot => citationIds.Contains(snapshot.CitationId))
                .ToDictionaryAsync(snapshot => snapshot.CitationId, cancellationToken)
            : new Dictionary<Guid, CitationNameSnapshot>();

        var officerProfiles = citationIds.Count > 0
            ? await dbContext.CitationOfficerProfiles
                .AsNoTracking()
                .Where(profile => citationIds.Contains(profile.CitationId))
                .ToDictionaryAsync(profile => profile.CitationId, cancellationToken)
            : new Dictionary<Guid, CitationOfficerProfile>();

        var vehicles = citationIds.Count > 0
            ? await dbContext.CitationVehicles
                .AsNoTracking()
                .Where(vehicle => citationIds.Contains(vehicle.CitationId))
                .ToDictionaryAsync(vehicle => vehicle.CitationId, cancellationToken)
            : new Dictionary<Guid, CitationVehicle>();

        var texasDetails = citationIds.Count > 0
            ? await dbContext.CitationTexasDetails
                .AsNoTracking()
                .Where(details => citationIds.Contains(details.CitationId))
                .ToDictionaryAsync(details => details.CitationId, cancellationToken)
            : new Dictionary<Guid, CitationTexasDetails>();

        return citations
            .Select(c =>
            {
                names.TryGetValue(c.DefendantNameId ?? Guid.Empty, out var name);
                snapshots.TryGetValue(c.Id, out var snapshot);
                officerProfiles.TryGetValue(c.Id, out var officerProfile);
                vehicles.TryGetValue(c.Id, out var vehicle);
                texasDetails.TryGetValue(c.Id, out var texasDetail);

                return c.ToDto(
                    defendantName: FormatName(name),
                    defendantNameRecordNumber: name?.RecordNumber,
                    atTimeOfName: snapshot is null ? null : CitationNameSnapshotBuilder.ToDto(snapshot),
                    officerProfile: officerProfile is null
                        ? null
                        : new CitationOfficerProfileDto(
                            officerProfile.SourceNameId,
                            officerProfile.SourceNameRecordNumber,
                            officerProfile.OfficerName,
                            officerProfile.Title,
                            officerProfile.BadgeOrIdentifier,
                            officerProfile.UnitNumber),
                    vehicle: vehicle is null
                        ? null
                        : new CitationVehicleDto(
                            vehicle.PlateNumber,
                            vehicle.PlateStateId,
                            vehicle.PlateYear,
                            vehicle.ModelYear,
                            vehicle.Make,
                            vehicle.Style,
                            vehicle.Color,
                            vehicle.IsCommercial,
                            vehicle.CarriesHazardousMaterial),
                    texasDetails: texasDetail is null
                        ? null
                        : new CitationTexasDetailsDto(
                            texasDetail.DocketNumber,
                            texasDetail.PageNumber,
                            texasDetail.ViolationSourceTypeId,
                            texasDetail.ViolationSection,
                            texasDetail.ViolationGroupId,
                            texasDetail.PrimaryViolationDescription,
                            texasDetail.SpeedMph,
                            texasDetail.ZoneMph,
                            texasDetail.SpeedBandId,
                            texasDetail.NarrativeOtherViolations,
                            texasDetail.OccurredAtText,
                            texasDetail.CourtAppearanceDateTime,
                            texasDetail.CourtAppearanceLocationId,
                            texasDetail.AffidavitSignedDate,
                            texasDetail.ComplainantSignatureText,
                            texasDetail.DefendantSignatureText,
                            texasDetail.AcceptedBondNotes,
                            texasDetail.ReceiptNumber));
            })
            .ToList();
    }

    private static string? FormatName(NameReadModel? name)
    {
        if (name is null)
            return null;

        if (name.NameType == NameTypes.Business)
            return name.LastOrBusinessName;

        if (string.IsNullOrWhiteSpace(name.FirstName))
            return name.LastOrBusinessName;

        return $"{name.LastOrBusinessName}, {name.FirstName}".Trim();
    }
}
