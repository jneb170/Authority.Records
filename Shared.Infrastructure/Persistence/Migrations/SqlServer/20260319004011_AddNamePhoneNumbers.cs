using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddNamePhoneNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OtherPhone",
                table: "Names",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPhoneExtension",
                table: "Names",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhone",
                table: "Names",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhoneExtension",
                table: "Names",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhone",
                table: "Names",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhoneExtension",
                table: "Names",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPhone",
                table: "NameReadModels",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherPhoneExtension",
                table: "NameReadModels",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhone",
                table: "NameReadModels",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhoneExtension",
                table: "NameReadModels",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhone",
                table: "NameReadModels",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WorkPhoneExtension",
                table: "NameReadModels",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OtherPhone",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "OtherPhoneExtension",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "PrimaryPhone",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "PrimaryPhoneExtension",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "WorkPhone",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "WorkPhoneExtension",
                table: "Names");

            migrationBuilder.DropColumn(
                name: "OtherPhone",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "OtherPhoneExtension",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "PrimaryPhone",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "PrimaryPhoneExtension",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "WorkPhone",
                table: "NameReadModels");

            migrationBuilder.DropColumn(
                name: "WorkPhoneExtension",
                table: "NameReadModels");
        }
    }
}
