using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddNameExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeceasedDate",
                table: "Names",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FbiNumber",
                table: "Names",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCitizen",
                table: "Names",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocalNumber",
                table: "Names",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "Names",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialSecurityNumber",
                table: "Names",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuffixId",
                table: "Names",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeceasedDate",
                table: "NameReadModels",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FbiNumber",
                table: "NameReadModels",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCitizen",
                table: "NameReadModels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LocalNumber",
                table: "NameReadModels",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlaceOfBirth",
                table: "NameReadModels",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SocialSecurityNumber",
                table: "NameReadModels",
                type: "nvarchar(11)",
                maxLength: 11,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SuffixId",
                table: "NameReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_DeceasedDate",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "DeceasedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_SuffixId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "SuffixId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NameReadModels_JurisdictionId_DeceasedDate",
                table: "NameReadModels");

            migrationBuilder.DropIndex(
                name: "IX_NameReadModels_JurisdictionId_SuffixId",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "DeceasedDate",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "FbiNumber",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "IsCitizen",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "LocalNumber",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "SocialSecurityNumber",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "SuffixId",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "DeceasedDate",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "FbiNumber",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "IsCitizen",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "LocalNumber",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "PlaceOfBirth",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "SocialSecurityNumber",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "SuffixId",
                table: "NameReadModels");
        }
    }
}
