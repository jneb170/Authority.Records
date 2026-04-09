using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChargesFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArrestChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArrestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrestChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OffenseName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    UcrCategory = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NibrsGroup = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CrimeAgainst = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UcrCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChargeLevel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StateClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsCitationEligible = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChargeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ArrestId",
                table: "ArrestChargeLinks",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ArrestId_ChargeId",
                table: "ArrestChargeLinks",
                columns: new[] { "ArrestId", "ChargeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ChargeId",
                table: "ArrestChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_IsActive",
                table: "Charges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_JurisdictionId_AgencyId_UcrCode_OffenseName",
                table: "Charges",
                columns: new[] { "JurisdictionId", "AgencyId", "UcrCode", "OffenseName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_ChargeId",
                table: "CitationChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_CitationId",
                table: "CitationChargeLinks",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_CitationId_ChargeId",
                table: "CitationChargeLinks",
                columns: new[] { "CitationId", "ChargeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_ChargeId",
                table: "IncidentChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_IncidentId",
                table: "IncidentChargeLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_IncidentId_ChargeId",
                table: "IncidentChargeLinks",
                columns: new[] { "IncidentId", "ChargeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArrestChargeLinks");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "CitationChargeLinks");

            migrationBuilder.DropTable(
                name: "IncidentChargeLinks");
        }
    }
}
