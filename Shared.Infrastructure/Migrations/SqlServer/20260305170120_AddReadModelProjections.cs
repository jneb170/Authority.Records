using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddReadModelProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArrestReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuspectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrestReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsIssued = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArrestCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReadModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_IncidentId",
                table: "ArrestReadModels",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_JurisdictionId",
                table: "ArrestReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_Status",
                table: "ArrestReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_IsIssued",
                table: "CitationReadModels",
                column: "IsIssued");

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_JurisdictionId",
                table: "CitationReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_UpdatedAtUtc",
                table: "CitationReadModels",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_JurisdictionId",
                table: "IncidentReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_Status",
                table: "IncidentReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_UpdatedAtUtc",
                table: "IncidentReadModels",
                column: "UpdatedAtUtc");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArrestReadModels");

            migrationBuilder.DropTable(
                name: "CitationReadModels");

            migrationBuilder.DropTable(
                name: "IncidentReadModels");
        }
    }
}
