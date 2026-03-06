using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations");

            migrationBuilder.AlterColumn<Guid>(
                name: "IncidentId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "CitationReadModels",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "CitationReadModels");

            migrationBuilder.AlterColumn<Guid>(
                name: "IncidentId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id");
        }
    }
}
