using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAggregateVersioning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AggregateId",
                table: "OutboxMessages",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "OutboxMessages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Incidents",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Citations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<Guid>(
                name: "AggregateId",
                table: "AuditTrailEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<long>(
                name: "AggregateVersion",
                table: "AuditTrailEntries",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "Version",
                table: "Arrests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "AggregateId",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "AggregateVersion",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Arrests");
        }
    }
}
