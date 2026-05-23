using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddCitationOfficerVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CitationOfficerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceNameRecordNumber = table.Column<long>(type: "bigint", nullable: true),
                    OfficerName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BadgeOrIdentifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UnitNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationOfficerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationOfficerProfiles_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitationVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlateNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PlateStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlateYear = table.Column<int>(type: "int", nullable: true),
                    ModelYear = table.Column<int>(type: "int", nullable: true),
                    Make = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Style = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCommercial = table.Column<bool>(type: "bit", nullable: false),
                    CarriesHazardousMaterial = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationVehicles_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CitationOfficerProfiles_CitationId",
                table: "CitationOfficerProfiles",
                column: "CitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationVehicles_CitationId",
                table: "CitationVehicles",
                column: "CitationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitationOfficerProfiles");

            migrationBuilder.DropTable(
                name: "CitationVehicles");
        }
    }
}
