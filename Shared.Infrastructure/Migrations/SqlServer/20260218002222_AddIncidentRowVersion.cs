using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class AddIncidentRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Arrests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IncidentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuspectName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Arrests");

            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "Incidents");
        }
    }
}
