using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddAgencyConfigurations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "IncidentNum",
                table: "Incidents",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "AgencyConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencySequenceCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CounterKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    NextValue = table.Column<long>(type: "bigint", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencySequenceCounters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_JurisdictionId_AgencyId_IncidentNum",
                table: "Incidents",
                columns: new[] { "JurisdictionId", "AgencyId", "IncidentNum" },
                unique: true,
                filter: "[IncidentNum] <> ''");

            migrationBuilder.CreateIndex(
                name: "IX_AgencyConfigurations_JurisdictionId_AgencyId_Key",
                table: "AgencyConfigurations",
                columns: new[] { "JurisdictionId", "AgencyId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencySequenceCounters_JurisdictionId_AgencyId_CounterKey_Year",
                table: "AgencySequenceCounters",
                columns: new[] { "JurisdictionId", "AgencyId", "CounterKey", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyConfigurations");

            migrationBuilder.DropTable(
                name: "AgencySequenceCounters");

            migrationBuilder.DropIndex(
                name: "IX_Incidents_JurisdictionId_AgencyId_IncidentNum",
                table: "Incidents");

            migrationBuilder.AlterColumn<string>(
                name: "IncidentNum",
                table: "Incidents",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
