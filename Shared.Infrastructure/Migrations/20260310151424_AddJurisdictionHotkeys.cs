using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJurisdictionHotkeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "HotkeyModify",
                table: "JurisdictionConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotkeyNew",
                table: "JurisdictionConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotkeyRelease",
                table: "JurisdictionConfigurations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HotkeySave",
                table: "JurisdictionConfigurations",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HotkeyModify",
                table: "JurisdictionConfigurations");

            migrationBuilder.DropColumn(
                name: "HotkeyNew",
                table: "JurisdictionConfigurations");

            migrationBuilder.DropColumn(
                name: "HotkeyRelease",
                table: "JurisdictionConfigurations");

            migrationBuilder.DropColumn(
                name: "HotkeySave",
                table: "JurisdictionConfigurations");
        }
    }
}
