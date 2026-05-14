using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddIncidentNum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.AlterColumn<string>(
            //    name: "IncidentNum",
            //    table: "Incidents",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(30)",
            //    oldMaxLength: 30,
            //    oldDefaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncidentNum",
                table: "Incidents",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            //migrationBuilder.AlterColumn<string>(
            //    name: "IncidentNum",
            //    table: "IncidentReadModels",
            //    type: "nvarchar(max)",
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(30)",
            //    oldMaxLength: 30,
            //    oldDefaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncidentNum",
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
                name: "IncidentNum",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentNum",
                table: "IncidentReadModels");

            //migrationBuilder.AlterColumn<string>(
            //    name: "IncidentNum",
            //    table: "Incidents",
            //    type: "nvarchar(30)",
            //    maxLength: 30,
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");

            //migrationBuilder.AlterColumn<string>(
            //    name: "IncidentNum",
            //    table: "IncidentReadModels",
            //    type: "nvarchar(30)",
            //    maxLength: 30,
            //    nullable: false,
            //    defaultValue: "",
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)");
        }
    }
}
