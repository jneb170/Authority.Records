using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddNarrative : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NarrativeLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NarrativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NarrativeLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NarrativeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NarrativeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NarrativeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NarrativeReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NarrativeReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Narratives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LockedByAgencyId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Narratives", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeLinkReadModels_JurisdictionId_OwnerType_OwnerId",
                table: "NarrativeLinkReadModels",
                columns: new[] { "JurisdictionId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeLinkReadModels_NarrativeId",
                table: "NarrativeLinkReadModels",
                column: "NarrativeId");

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeLinks_NarrativeId_OwnerType_OwnerId",
                table: "NarrativeLinks",
                columns: new[] { "NarrativeId", "OwnerType", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeLinks_OwnerType_OwnerId",
                table: "NarrativeLinks",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_NarrativeReadModels_JurisdictionId",
                table: "NarrativeReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Narratives_JurisdictionId",
                table: "Narratives",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_Narratives_RecordNumber",
                table: "Narratives",
                column: "RecordNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NarrativeLinkReadModels");

            migrationBuilder.DropTable(
                name: "NarrativeLinks");

            migrationBuilder.DropTable(
                name: "NarrativeReadModels");

            migrationBuilder.DropTable(
                name: "Narratives");
        }
    }
}
