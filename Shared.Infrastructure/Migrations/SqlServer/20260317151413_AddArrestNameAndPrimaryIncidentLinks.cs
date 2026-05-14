using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddArrestNameAndPrimaryIncidentLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SuspectName",
                table: "Arrests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "SuspectName",
                table: "ArrestReadModels",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "NameId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryIncidentId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "NameId",
                table: "ArrestReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PrimaryIncidentId",
                table: "ArrestReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_NameId",
                table: "Arrests",
                column: "NameId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_PrimaryIncidentId",
                table: "Arrests",
                column: "PrimaryIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_NameId",
                table: "ArrestReadModels",
                column: "NameId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_PrimaryIncidentId",
                table: "ArrestReadModels",
                column: "PrimaryIncidentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arrests_Incidents_PrimaryIncidentId",
                table: "Arrests",
                column: "PrimaryIncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Arrests_Names_NameId",
                table: "Arrests",
                column: "NameId",
                principalTable: "Names",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arrests_Incidents_PrimaryIncidentId",
                table: "Arrests");

            migrationBuilder.DropForeignKey(
                name: "FK_Arrests_Names_NameId",
                table: "Arrests");

            migrationBuilder.DropIndex(
                name: "IX_Arrests_NameId",
                table: "Arrests");

            migrationBuilder.DropIndex(
                name: "IX_Arrests_PrimaryIncidentId",
                table: "Arrests");

            migrationBuilder.DropIndex(
                name: "IX_ArrestReadModels_NameId",
                table: "ArrestReadModels");

            migrationBuilder.DropIndex(
                name: "IX_ArrestReadModels_PrimaryIncidentId",
                table: "ArrestReadModels");

            migrationBuilder.DropColumn(
                name: "NameId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "PrimaryIncidentId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "NameId",
                table: "ArrestReadModels");

            migrationBuilder.DropColumn(
                name: "PrimaryIncidentId",
                table: "ArrestReadModels");

            migrationBuilder.AlterColumn<string>(
                name: "SuspectName",
                table: "Arrests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuspectName",
                table: "ArrestReadModels",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
