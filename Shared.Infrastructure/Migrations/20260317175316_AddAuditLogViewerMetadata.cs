using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditLogViewerMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "JurisdictionId",
                table: "AuditTrailEntries",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "ActionType",
                table: "AuditTrailEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Message",
                table: "AuditTrailEntries",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RecordType",
                table: "AuditTrailEntries",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "AuditTrailEntries",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AuditTrailEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_ActionType",
                table: "AuditTrailEntries",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_JurisdictionId_OccurredOnUtc",
                table: "AuditTrailEntries",
                columns: new[] { "JurisdictionId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_RecordType",
                table: "AuditTrailEntries",
                column: "RecordType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_Severity",
                table: "AuditTrailEntries",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_UserId",
                table: "AuditTrailEntries",
                column: "UserId");

            migrationBuilder.Sql(
                """
                UPDATE AuditTrailEntries
                SET
                    Severity =
                        CASE
                            WHEN EventType = 'SystemLockExpired' THEN 'Warning'
                            WHEN EventType LIKE '%SoftDeletedDomainEvent' THEN 'Warning'
                            ELSE 'Information'
                        END,
                    ActionType =
                        CASE
                            WHEN EventType = 'SystemLockExpired' THEN 'LockExpired'
                            WHEN EventType LIKE 'LockAcquiredDomainEvent%' THEN 'LockAcquired'
                            WHEN EventType LIKE 'LockReleasedDomainEvent%' THEN 'LockReleased'
                            WHEN EventType LIKE 'LifecycleStatusChangedDomainEvent%' THEN 'StatusChanged'
                            WHEN EventType LIKE '%DetailsUpdatedDomainEvent' THEN 'Updated'
                            WHEN EventType LIKE '%UpdatedDomainEvent' THEN 'Updated'
                            WHEN EventType LIKE '%CreatedDomainEvent' THEN 'Created'
                            WHEN EventType LIKE '%SoftDeletedDomainEvent' THEN 'SoftDeleted'
                            WHEN EventType LIKE '%RestoredDomainEvent' THEN 'Restored'
                            ELSE EventType
                        END,
                    RecordType =
                        CASE
                            WHEN EventType = 'SystemLockExpired' THEN COALESCE(JSON_VALUE(Payload, '$.AggregateType'), 'System')
                            WHEN EventType LIKE 'LockAcquiredDomainEvent%' THEN 'Unknown'
                            WHEN EventType LIKE 'LockReleasedDomainEvent%' THEN 'Unknown'
                            WHEN EventType LIKE 'LifecycleStatusChangedDomainEvent%' THEN 'Unknown'
                            WHEN EventType LIKE '%DetailsUpdatedDomainEvent' THEN REPLACE(EventType, 'DetailsUpdatedDomainEvent', '')
                            WHEN EventType LIKE '%UpdatedDomainEvent' THEN REPLACE(EventType, 'UpdatedDomainEvent', '')
                            WHEN EventType LIKE '%CreatedDomainEvent' THEN REPLACE(EventType, 'CreatedDomainEvent', '')
                            WHEN EventType LIKE '%SoftDeletedDomainEvent' THEN REPLACE(EventType, 'SoftDeletedDomainEvent', '')
                            WHEN EventType LIKE '%RestoredDomainEvent' THEN REPLACE(EventType, 'RestoredDomainEvent', '')
                            ELSE 'System'
                        END,
                    UserId =
                        CASE
                            WHEN JSON_VALUE(Payload, '$.ChangedByUserId') IS NOT NULL THEN TRY_CONVERT(uniqueidentifier, JSON_VALUE(Payload, '$.ChangedByUserId'))
                            WHEN JSON_VALUE(Payload, '$.UserId') IS NOT NULL THEN TRY_CONVERT(uniqueidentifier, JSON_VALUE(Payload, '$.UserId'))
                            ELSE NULL
                        END;
                """);

            migrationBuilder.Sql(
                """
                UPDATE AuditTrailEntries
                SET Message =
                    CASE
                        WHEN ActionType = 'LockExpired' THEN 'System released an expired ' + LOWER(RecordType) + ' lock.'
                        WHEN ActionType = 'StatusChanged' THEN RecordType + ' status changed.'
                        WHEN ActionType = 'Created' THEN RecordType + ' created.'
                        WHEN ActionType = 'Updated' THEN RecordType + ' updated.'
                        WHEN ActionType = 'SoftDeleted' THEN RecordType + ' soft deleted.'
                        WHEN ActionType = 'Restored' THEN RecordType + ' restored.'
                        WHEN ActionType = 'LockAcquired' THEN RecordType + ' lock acquired.'
                        WHEN ActionType = 'LockReleased' THEN RecordType + ' lock released.'
                        ELSE EventType
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_ActionType",
                table: "AuditTrailEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_JurisdictionId_OccurredOnUtc",
                table: "AuditTrailEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_RecordType",
                table: "AuditTrailEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_Severity",
                table: "AuditTrailEntries");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_UserId",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "ActionType",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "Message",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "RecordType",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AuditTrailEntries");

            migrationBuilder.Sql(
                """
                UPDATE AuditTrailEntries
                SET JurisdictionId = '00000000-0000-0000-0000-000000000000'
                WHERE JurisdictionId IS NULL;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "JurisdictionId",
                table: "AuditTrailEntries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
