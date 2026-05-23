using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AppAssignedRecordNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Names",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "ABS(RANDOM())");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "ABS(RANDOM())");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Incidents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "ABS(RANDOM())");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Citations",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "ABS(RANDOM())");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Arrests",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldDefaultValueSql: "ABS(RANDOM())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Names",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "ABS(RANDOM())",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Locations",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "ABS(RANDOM())",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Incidents",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "ABS(RANDOM())",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Citations",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "ABS(RANDOM())",
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<long>(
                name: "RecordNumber",
                table: "Arrests",
                type: "INTEGER",
                nullable: false,
                defaultValueSql: "ABS(RANDOM())",
                oldClrType: typeof(long),
                oldType: "INTEGER");
        }
    }
}
