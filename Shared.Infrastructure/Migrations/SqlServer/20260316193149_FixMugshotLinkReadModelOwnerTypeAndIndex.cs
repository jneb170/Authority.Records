using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class FixMugshotLinkReadModelOwnerTypeAndIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "OwnerType",
                table: "MugshotLinkReadModels",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_MugshotLinkReadModels_JurisdictionId_OwnerType_OwnerId",
                table: "MugshotLinkReadModels",
                columns: new[] { "JurisdictionId", "OwnerType", "OwnerId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MugshotLinkReadModels_JurisdictionId_OwnerType_OwnerId",
                table: "MugshotLinkReadModels");

            migrationBuilder.AlterColumn<string>(
                name: "OwnerType",
                table: "MugshotLinkReadModels",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);
        }
    }
}
