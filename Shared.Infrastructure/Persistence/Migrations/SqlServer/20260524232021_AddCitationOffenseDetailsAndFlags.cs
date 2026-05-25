using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddCitationOffenseDetailsAndFlags : Migration
    {
        // The jurisdiction-neutral offense columns that move out of CitationTexasDetails into the
        // new CitationOffenseDetails table. The data copy below reuses each source row's Id/tenant
        // keys so existing citations keep their offense data after the move.
        private const string CopyOffenseDataForward = @"
INSERT INTO CitationOffenseDetails (Id, JurisdictionId, AgencyId, CitationId, ViolationSourceTypeId, ViolationSection, ViolationGroupId, PrimaryViolationDescription, SpeedMph, ZoneMph, SpeedBandId, NarrativeOtherViolations, OccurredAtText, CourtAppearanceDateTime, CourtAppearanceLocationId, AffidavitSignedDate, ComplainantSignatureText, DefendantSignatureText, AcceptedBondNotes, ReceiptNumber)
SELECT Id, JurisdictionId, AgencyId, CitationId, ViolationSourceTypeId, ViolationSection, ViolationGroupId, PrimaryViolationDescription, SpeedMph, ZoneMph, SpeedBandId, NarrativeOtherViolations, OccurredAtText, CourtAppearanceDateTime, CourtAppearanceLocationId, AffidavitSignedDate, ComplainantSignatureText, DefendantSignatureText, AcceptedBondNotes, ReceiptNumber
FROM CitationTexasDetails;";

        // Reverse copy used by Down(): restore the offense scalars onto CitationTexasDetails before
        // the offense table is dropped.
        private const string CopyOffenseDataBack = @"
UPDATE CitationTexasDetails
SET ViolationSourceTypeId = (SELECT o.ViolationSourceTypeId FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    ViolationSection = (SELECT o.ViolationSection FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    ViolationGroupId = (SELECT o.ViolationGroupId FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    PrimaryViolationDescription = (SELECT o.PrimaryViolationDescription FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    SpeedMph = (SELECT o.SpeedMph FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    ZoneMph = (SELECT o.ZoneMph FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    SpeedBandId = (SELECT o.SpeedBandId FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    NarrativeOtherViolations = (SELECT o.NarrativeOtherViolations FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    OccurredAtText = (SELECT o.OccurredAtText FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    CourtAppearanceDateTime = (SELECT o.CourtAppearanceDateTime FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    CourtAppearanceLocationId = (SELECT o.CourtAppearanceLocationId FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    AffidavitSignedDate = (SELECT o.AffidavitSignedDate FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    ComplainantSignatureText = (SELECT o.ComplainantSignatureText FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    DefendantSignatureText = (SELECT o.DefendantSignatureText FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    AcceptedBondNotes = (SELECT o.AcceptedBondNotes FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId),
    ReceiptNumber = (SELECT o.ReceiptNumber FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId)
WHERE EXISTS (SELECT 1 FROM CitationOffenseDetails o WHERE o.CitationId = CitationTexasDetails.CitationId);";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CitationOffenseDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViolationSourceTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ViolationSection = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ViolationGroupId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrimaryViolationDescription = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    SpeedMph = table.Column<int>(type: "int", nullable: true),
                    ZoneMph = table.Column<int>(type: "int", nullable: true),
                    SpeedBandId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NarrativeOtherViolations = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OccurredAtText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CourtAppearanceDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CourtAppearanceLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AffidavitSignedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComplainantSignatureText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DefendantSignatureText = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    AcceptedBondNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationOffenseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationOffenseDetails_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitationViolationFlags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SourceChargeLinkId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationViolationFlags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationViolationFlags_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitationOffenseDetails_CitationId",
                table: "CitationOffenseDetails",
                column: "CitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationViolationFlags_CitationId_Key",
                table: "CitationViolationFlags",
                columns: new[] { "CitationId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationViolationFlags_Key",
                table: "CitationViolationFlags",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_CitationViolationFlags_SourceChargeLinkId",
                table: "CitationViolationFlags",
                column: "SourceChargeLinkId");

            // Move existing offense data into the new table BEFORE dropping the source columns.
            migrationBuilder.Sql(CopyOffenseDataForward);

            migrationBuilder.DropColumn(name: "AcceptedBondNotes", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "AffidavitSignedDate", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ComplainantSignatureText", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "CourtAppearanceDateTime", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "CourtAppearanceLocationId", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "DefendantSignatureText", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "NarrativeOtherViolations", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "OccurredAtText", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "PrimaryViolationDescription", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ReceiptNumber", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "SpeedBandId", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "SpeedMph", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ViolationGroupId", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ViolationSection", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ViolationSourceTypeId", table: "CitationTexasDetails");
            migrationBuilder.DropColumn(name: "ZoneMph", table: "CitationTexasDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "AcceptedBondNotes", table: "CitationTexasDetails", type: "nvarchar(500)", maxLength: 500, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "AffidavitSignedDate", table: "CitationTexasDetails", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ComplainantSignatureText", table: "CitationTexasDetails", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<DateTime>(name: "CourtAppearanceDateTime", table: "CitationTexasDetails", type: "datetime2", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "CourtAppearanceLocationId", table: "CitationTexasDetails", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<string>(name: "DefendantSignatureText", table: "CitationTexasDetails", type: "nvarchar(150)", maxLength: 150, nullable: true);
            migrationBuilder.AddColumn<string>(name: "NarrativeOtherViolations", table: "CitationTexasDetails", type: "nvarchar(1000)", maxLength: 1000, nullable: true);
            migrationBuilder.AddColumn<string>(name: "OccurredAtText", table: "CitationTexasDetails", type: "nvarchar(250)", maxLength: 250, nullable: true);
            migrationBuilder.AddColumn<string>(name: "PrimaryViolationDescription", table: "CitationTexasDetails", type: "nvarchar(250)", maxLength: 250, nullable: true);
            migrationBuilder.AddColumn<string>(name: "ReceiptNumber", table: "CitationTexasDetails", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "SpeedBandId", table: "CitationTexasDetails", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<int>(name: "SpeedMph", table: "CitationTexasDetails", type: "int", nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ViolationGroupId", table: "CitationTexasDetails", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<string>(name: "ViolationSection", table: "CitationTexasDetails", type: "nvarchar(50)", maxLength: 50, nullable: true);
            migrationBuilder.AddColumn<Guid>(name: "ViolationSourceTypeId", table: "CitationTexasDetails", type: "uniqueidentifier", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ZoneMph", table: "CitationTexasDetails", type: "int", nullable: true);

            // Restore the moved data before dropping the offense table.
            migrationBuilder.Sql(CopyOffenseDataBack);

            migrationBuilder.DropTable(name: "CitationOffenseDetails");
            migrationBuilder.DropTable(name: "CitationViolationFlags");
        }
    }
}
