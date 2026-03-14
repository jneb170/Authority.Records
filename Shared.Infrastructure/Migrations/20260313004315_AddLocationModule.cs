using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryLocationId",
                table: "Names",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryLocationId",
                table: "Names",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryLocationId",
                table: "NameReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SecondaryLocationId",
                table: "NameReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "IncidentReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "CitationReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "ArrestReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LocationReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordNumber = table.Column<long>(type: "bigint", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreDirectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StreetTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostDirectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AptSuite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Coordinates = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommonPlaceName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "20000, 1"),
                    StreetNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PreDirectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StreetTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PostDirectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Zip = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    AptSuite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Coordinates = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CommonPlaceName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_City",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_CommonPlaceName",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "CommonPlaceName" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_StateId",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_StreetAddress",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "StreetAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_Zip",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "Zip" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_City",
                table: "Locations",
                columns: new[] { "JurisdictionId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_CommonPlaceName",
                table: "Locations",
                columns: new[] { "JurisdictionId", "CommonPlaceName" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_StateId",
                table: "Locations",
                columns: new[] { "JurisdictionId", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_StreetAddress",
                table: "Locations",
                columns: new[] { "JurisdictionId", "StreetAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_Zip",
                table: "Locations",
                columns: new[] { "JurisdictionId", "Zip" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_RecordNumber",
                table: "Locations",
                column: "RecordNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LocationReadModels");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropColumn(
                name: "PrimaryLocationId",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "SecondaryLocationId",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "PrimaryLocationId",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "SecondaryLocationId",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "IncidentReadModels");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "CitationReadModels");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "ArrestReadModels");
        }
    }
}
