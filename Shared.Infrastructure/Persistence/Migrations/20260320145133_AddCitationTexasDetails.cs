using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCitationTexasDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CitationTexasDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocketNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PageNumber = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
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
                    table.PrimaryKey("PK_CitationTexasDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationTexasDetails_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitationTexasDetails_CitationId",
                table: "CitationTexasDetails",
                column: "CitationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitationTexasDetails");
        }
    }
}
