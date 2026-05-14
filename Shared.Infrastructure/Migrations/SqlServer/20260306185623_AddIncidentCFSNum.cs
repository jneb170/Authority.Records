using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddIncidentCFSNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CFSNum",
                table: "Incidents",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CFSNum",
                table: "IncidentReadModels",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CFSNum",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CFSNum",
                table: "IncidentReadModels");
        }
    }
}
