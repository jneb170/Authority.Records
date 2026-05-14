using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddNameModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NameReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RecordNumber = table.Column<long>(type: "bigint", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeightInches = table.Column<int>(type: "int", nullable: true),
                    WeightLbs = table.Column<int>(type: "int", nullable: true),
                    HairColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NameReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Names",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NameType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeightInches = table.Column<int>(type: "int", nullable: true),
                    WeightLbs = table.Column<int>(type: "int", nullable: true),
                    HairColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "10000, 1"),
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
                    table.PrimaryKey("PK_Names", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_DriversLicenseNumber",
                table: "NameReadModels",
                column: "DriversLicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_DateOfBirth",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_EyeColorId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "EyeColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_FirstName",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_HairColorId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "HairColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_HeightInches",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "HeightInches" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_LastOrBusinessName",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "LastOrBusinessName" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_NameType",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "NameType" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_RaceId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "RaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_SexId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "SexId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_WeightLbs",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "WeightLbs" });

            migrationBuilder.CreateIndex(
                name: "IX_Names_RecordNumber",
                table: "Names",
                column: "RecordNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NameReadModels");

            migrationBuilder.DropTable(
                name: "Names");
        }
    }
}
