using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddArrestNumCitationNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CitationNum",
                table: "Citations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CitationNum",
                table: "CitationReadModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrestNum",
                table: "Arrests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ArrestNum",
                table: "ArrestReadModels",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CitationNum",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "CitationNum",
                table: "CitationReadModels");

            migrationBuilder.DropColumn(
                name: "ArrestNum",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "ArrestNum",
                table: "ArrestReadModels");
        }
    }
}
