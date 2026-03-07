using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecordNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "Incidents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "10000, 1");

            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "IncidentReadModels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "Citations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "10000, 1");

            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "CitationReadModels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "Arrests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("SqlServer:Identity", "10000, 1");

            migrationBuilder.AddColumn<long>(
                name: "RecordNumber",
                table: "ArrestReadModels",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_RecordNumber",
                table: "Incidents",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Citations_RecordNumber",
                table: "Citations",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_RecordNumber",
                table: "Arrests",
                column: "RecordNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Incidents_RecordNumber",
                table: "Incidents");

            migrationBuilder.DropIndex(
                name: "IX_Citations_RecordNumber",
                table: "Citations");

            migrationBuilder.DropIndex(
                name: "IX_Arrests_RecordNumber",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "IncidentReadModels");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "CitationReadModels");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "RecordNumber",
                table: "ArrestReadModels");
        }
    }
}
