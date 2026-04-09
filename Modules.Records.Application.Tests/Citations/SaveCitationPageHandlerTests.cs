using Microsoft.EntityFrameworkCore;
using Modules.Records.Application.Citations.Commands.SaveCitationPage;
using Modules.Records.Application.DTOs;
using Modules.Records.Application.Tests.Infrastructure;
using Modules.Records.Domain.Common;
using Modules.Records.Domain.Common.Implementations;
using Modules.Records.Domain.Entities;
using Modules.Records.Domain.Factories;
using Modules.Records.Domain.ValueObjects;

namespace Modules.Records.Application.Tests.Citations;

public sealed class SaveCitationPageHandlerTests
{
    [Fact]
    public async Task Handle_UpdatesCitationLinksAndCharges_WithSingleSaveCall()
    {
        await using var db = RecordPageSaveTestDbContextFactory.Create();

        var jurisdictionId = Guid.NewGuid();
        var agencyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var locationId = Guid.NewGuid();
        var defendantNameId = Guid.NewGuid();

        var tenantProvider = new TestTenantProvider(jurisdictionId, agencyId, userId);
        var modificationContext = new UserModificationContext(userId);
        var handler = new SaveCitationPageHandler(db, tenantProvider, modificationContext);

        var citation = new Citation(jurisdictionId, agencyId, "Original citation", DateTime.UtcNow.AddDays(-2), "CT-1");
        var defendantName = new Name(
            jurisdictionId,
            agencyId,
            NameTypes.Person,
            "Driver",
            "Jamie",
            "Q",
            null,
            null,
            new DateTime(1991, 7, 14),
            "TXD123456",
            null,
            68,
            145,
            null,
            null,
            null,
            "Austin",
            "FBI-1",
            "LOCAL-1",
            "111-22-3333",
            true,
            null,
            "555-0100");
        db.Entry(defendantName).Property(nameof(Name.Id)).CurrentValue = defendantNameId;
        var incidentToRemove = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "Old incident",
                IncidentNum = "INC-OLD",
                LocalNum = "LOC-OLD"
            }
        });
        var incidentToAdd = new IncidentFactory().Create(new CreateIncidentRequest
        {
            JurisdictionId = jurisdictionId,
            AgencyId = agencyId,
            Details = new IncidentDetails
            {
                Description = "New incident",
                IncidentNum = "INC-NEW",
                LocalNum = "LOC-NEW"
            }
        });
        var chargeToRemove = new Charge(jurisdictionId, agencyId, "Remove", "Cat", "Group", "Person", "020", "Misdemeanor", null, true);
        var chargeToAdd = new Charge(jurisdictionId, agencyId, "Add", "Cat", "Group", "Person", "021", "Felony", null, true);

        db.Citations.Add(citation);
        db.Names.Add(defendantName);
        db.Incidents.AddRange(incidentToRemove, incidentToAdd);
        db.Charges.AddRange(chargeToRemove, chargeToAdd);
        db.IncidentCitationLinks.Add(new IncidentCitationLink(jurisdictionId, incidentToRemove.Id, citation.Id, userId));
        db.CitationChargeLinks.Add(new CitationChargeLink(jurisdictionId, citation.Id, chargeToRemove.Id, userId));
        await db.SaveChangesAsync(CancellationToken.None);
        db.ResetSaveChangesCallCount();

        var command = new SaveCitationPageCommand(
            citation.Id,
            defendantNameId,
            "Updated citation",
            DateTime.UtcNow.AddHours(-3),
            Guid.NewGuid(),
            "CT-200",
            locationId,
            new NameSnapshotInput(
                NameTypes.Person,
                "Updated",
                "Taylor",
                null,
                null,
                null,
                new DateTime(1992, 5, 6),
                "DL-200",
                null,
                71,
                190,
                null,
                null,
                null,
                "Plano",
                "FBI-200",
                "LOCAL-200",
                "555-0100",
                "12",
                "555-0200",
                "34",
                "555-0300",
                "56",
                "222-33-4444",
                true,
                null,
                new NameSnapshotAddressDto(null, null, "New primary address"),
                new NameSnapshotAddressDto(null, null, "New secondary address")),
            new CitationOfficerProfileInput(
                Guid.NewGuid(),
                8877,
                "Officer Riley",
                "Officer",
                "B-42",
                "U-7"),
            new CitationTexasDetailsInput(
                "DKT-77",
                "12",
                Guid.NewGuid(),
                "545.351",
                Guid.NewGuid(),
                "Speeding",
                72,
                55,
                Guid.NewGuid(),
                "Unsafe speed for posted conditions",
                "IH-35 frontage road",
                DateTime.UtcNow.AddDays(21),
                Guid.NewGuid(),
                DateTime.UtcNow.Date,
                "Officer Riley",
                "Jamie Driver",
                "Bond accepted at window 2",
                "RCPT-12"),
            new CitationVehicleInput(
                "TX-ABC123",
                Guid.NewGuid(),
                2025,
                2022,
                "Ford",
                "SUV",
                "Blue",
                true,
                false),
            [incidentToAdd.Id],
            [incidentToRemove.Id],
            [chargeToAdd.Id],
            [chargeToRemove.Id]);

        await handler.Handle(command, CancellationToken.None);

        Assert.Equal(1, db.SaveChangesCallCount);

        var savedCitation = await db.Citations.SingleAsync(c => c.Id == citation.Id);
        Assert.Equal("Updated citation", savedCitation.Description);
        Assert.Equal("CT-200", savedCitation.CitationNum);
        Assert.Equal(locationId, savedCitation.LocationId);
        Assert.Equal(defendantNameId, savedCitation.DefendantNameId);

        var snapshot = await db.CitationNameSnapshots.SingleAsync(s => s.CitationId == citation.Id);
        Assert.Equal("Updated", snapshot.LastOrBusinessName);
        Assert.Equal("Taylor", snapshot.FirstName);
        Assert.Equal("DL-200", snapshot.DriversLicenseNumber);
        Assert.Equal("Plano", snapshot.PlaceOfBirth);
        Assert.Equal("New primary address", snapshot.PrimaryLocationAddress);
        Assert.Equal("New secondary address", snapshot.SecondaryLocationAddress);
        Assert.True(snapshot.IsCitizen);
        Assert.Equal(userId, snapshot.LastCopiedByUserId);
        Assert.NotNull(snapshot.LastCopiedAtUtc);

        var officerProfile = await db.CitationOfficerProfiles.SingleAsync(p => p.CitationId == citation.Id);
        Assert.Equal("Officer Riley", officerProfile.OfficerName);
        Assert.Equal("Officer", officerProfile.Title);
        Assert.Equal("B-42", officerProfile.BadgeOrIdentifier);
        Assert.Equal("U-7", officerProfile.UnitNumber);
        Assert.Equal(8877, officerProfile.SourceNameRecordNumber);

        var texasDetails = await db.CitationTexasDetails.SingleAsync(d => d.CitationId == citation.Id);
        Assert.Equal("DKT-77", texasDetails.DocketNumber);
        Assert.Equal("12", texasDetails.PageNumber);
        Assert.Equal("545.351", texasDetails.ViolationSection);
        Assert.Equal("Speeding", texasDetails.PrimaryViolationDescription);
        Assert.Equal(72, texasDetails.SpeedMph);
        Assert.Equal(55, texasDetails.ZoneMph);
        Assert.Equal("Unsafe speed for posted conditions", texasDetails.NarrativeOtherViolations);
        Assert.Equal("IH-35 frontage road", texasDetails.OccurredAtText);
        Assert.Equal("Officer Riley", texasDetails.ComplainantSignatureText);
        Assert.Equal("Jamie Driver", texasDetails.DefendantSignatureText);
        Assert.Equal("Bond accepted at window 2", texasDetails.AcceptedBondNotes);
        Assert.Equal("RCPT-12", texasDetails.ReceiptNumber);

        var vehicle = await db.CitationVehicles.SingleAsync(v => v.CitationId == citation.Id);
        Assert.Equal("TX-ABC123", vehicle.PlateNumber);
        Assert.Equal(2025, vehicle.PlateYear);
        Assert.Equal(2022, vehicle.ModelYear);
        Assert.Equal("Ford", vehicle.Make);
        Assert.Equal("SUV", vehicle.Style);
        Assert.Equal("Blue", vehicle.Color);
        Assert.True(vehicle.IsCommercial);
        Assert.False(vehicle.CarriesHazardousMaterial);

        var linkedIncidentIds = await db.IncidentCitationLinks
            .Where(link => link.CitationId == citation.Id)
            .Select(link => link.IncidentId)
            .ToListAsync();
        Assert.Single(linkedIncidentIds);
        Assert.Contains(incidentToAdd.Id, linkedIncidentIds);

        var chargeLinkIds = await db.CitationChargeLinks
            .Where(link => link.CitationId == citation.Id)
            .Select(link => link.ChargeId)
            .ToListAsync();
        Assert.Single(chargeLinkIds);
        Assert.Contains(chargeToAdd.Id, chargeLinkIds);
    }
}
