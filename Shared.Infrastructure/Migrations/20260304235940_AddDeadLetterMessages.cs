using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeadLetterMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Incidents");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Incidents",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Incidents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Incidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "IncidentId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Citations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalized",
                table: "Citations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsIssued",
                table: "Citations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "IssueDate",
                table: "Citations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Arrests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFinalized",
                table: "Arrests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "JurisdictionId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAtUtc",
                table: "Arrests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LockedByUserId",
                table: "Arrests",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Arrests",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Arrests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeadLetterMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OriginalMessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeadLetteredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    RequeuedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JurisdictionConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MustCloseAllArrests = table.Column<bool>(type: "bit", nullable: false),
                    MustCloseAllCitations = table.Column<bool>(type: "bit", nullable: false),
                    MustCloseArrestsBeforeIncidentClose = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JurisdictionConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessingStartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsFailedPermanently = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    NextRetryOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Citations_IncidentId",
                table: "Citations",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_IncidentId",
                table: "Arrests",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_DeadLetteredOnUtc",
                table: "DeadLetterMessages",
                column: "DeadLetteredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_JurisdictionId",
                table: "DeadLetterMessages",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_DeadLetterMessages_OriginalMessageId",
                table: "DeadLetterMessages",
                column: "OriginalMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_JurisdictionConfigurations_JurisdictionId",
                table: "JurisdictionConfigurations",
                column: "JurisdictionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Arrests_Incidents_IncidentId",
                table: "Arrests",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations",
                column: "IncidentId",
                principalTable: "Incidents",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arrests_Incidents_IncidentId",
                table: "Arrests");

            migrationBuilder.DropForeignKey(
                name: "FK_Citations_Incidents_IncidentId",
                table: "Citations");

            migrationBuilder.DropTable(
                name: "DeadLetterMessages");

            migrationBuilder.DropTable(
                name: "JurisdictionConfigurations");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_Citations_IncidentId",
                table: "Citations");

            migrationBuilder.DropIndex(
                name: "IX_Arrests_IncidentId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Incidents");

            migrationBuilder.DropColumn(
                name: "IncidentId",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IsFinalized",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IsIssued",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IssueDate",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "IsFinalized",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "JurisdictionId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "LockedAtUtc",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "LockedByUserId",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Arrests");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Incidents");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Incidents",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }
    }
}
