using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddM2MAssociations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arrests_Incidents_IncidentId",
                table: "Arrests");

            migrationBuilder.DropForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations");

            migrationBuilder.DropIndex(
                name: "IX_Citations_IncidentId",
                table: "Citations");

            migrationBuilder.DropIndex(
                name: "IX_Arrests_IncidentId",
                table: "Arrests");

            migrationBuilder.DropIndex(
                name: "IX_ArrestReadModels_IncidentId",
                table: "ArrestReadModels");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "CitationReadModels");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "ArrestReadModels");

            migrationBuilder.AddColumn<int>(
                name: "CitationCount",
                table: "IncidentReadModels",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "IncidentArrestLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentRecordNumber = table.Column<long>(type: "bigint", nullable: false),
                    IncidentNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArrestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentArrestLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentArrestLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArrestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_IncidentArrestLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCitationLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentRecordNumber = table.Column<long>(type: "bigint", nullable: false),
                    IncidentNum = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCitationLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCitationLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_IncidentCitationLinks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinkReadModels_ArrestId",
                table: "IncidentArrestLinkReadModels",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinkReadModels_IncidentId",
                table: "IncidentArrestLinkReadModels",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_ArrestId",
                table: "IncidentArrestLinks",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_IncidentId",
                table: "IncidentArrestLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_IncidentId_ArrestId",
                table: "IncidentArrestLinks",
                columns: new[] { "IncidentId", "ArrestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinkReadModels_CitationId",
                table: "IncidentCitationLinkReadModels",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinkReadModels_IncidentId",
                table: "IncidentCitationLinkReadModels",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_CitationId",
                table: "IncidentCitationLinks",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_IncidentId",
                table: "IncidentCitationLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_IncidentId_CitationId",
                table: "IncidentCitationLinks",
                columns: new[] { "IncidentId", "CitationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentArrestLinkReadModels");

            migrationBuilder.DropTable(
                name: "IncidentArrestLinks");

            migrationBuilder.DropTable(
                name: "IncidentCitationLinkReadModels");

            migrationBuilder.DropTable(
                name: "IncidentCitationLinks");

            migrationBuilder.DropColumn(
                name: "CitationCount",
                table: "IncidentReadModels");

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "CitationReadModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "ArrestReadModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Citations_IncidentId",
                table: "Citations",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_IncidentId",
                table: "Arrests",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_IncidentId",
                table: "ArrestReadModels",
                column: "IncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arrests_Incidents_IncidentId",
                table: "Arrests",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
