using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregateRootAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Incidents",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Incidents",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Citations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Citations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Arrests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Arrests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModifiedBy",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                table: "Arrests");
        }
    }
}
