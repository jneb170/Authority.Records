using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddAuditAgencyId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AgencyId",
                table: "AuditTrailEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_JurisdictionId_AgencyId_OccurredOnUtc",
                table: "AuditTrailEntries",
                columns: new[] { "JurisdictionId", "AgencyId", "OccurredOnUtc" });

            // Backfill (issue #118): rows about an incident, arrest or citation belong to that
            // record's agency, and narrative rows to the agency of the record they are attached
            // to. Everything else stays jurisdiction-wide (NULL), which is correct for the shared
            // name and location index.
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""Incidents"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"") WHERE ""AgencyId"" IS NULL AND EXISTS (SELECT 1 FROM ""Incidents"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"");");
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""Arrests"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"") WHERE ""AgencyId"" IS NULL AND EXISTS (SELECT 1 FROM ""Arrests"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"");");
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""Citations"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"") WHERE ""AgencyId"" IS NULL AND EXISTS (SELECT 1 FROM ""Citations"" o WHERE o.""Id"" = ""AuditTrailEntries"".""AggregateId"");");
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Incidents"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Incident' LIMIT 1) WHERE ""AgencyId"" IS NULL AND ""RecordType"" = 'Narrative' AND (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Incidents"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Incident' LIMIT 1) IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Arrests"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Arrest' LIMIT 1) WHERE ""AgencyId"" IS NULL AND ""RecordType"" = 'Narrative' AND (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Arrests"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Arrest' LIMIT 1) IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""AgencyId"" = (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Citations"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Citation' LIMIT 1) WHERE ""AgencyId"" IS NULL AND ""RecordType"" = 'Narrative' AND (SELECT o.""AgencyId"" FROM ""NarrativeLinks"" l JOIN ""Citations"" o ON o.""Id"" = l.""OwnerId"" WHERE l.""NarrativeId"" = ""AuditTrailEntries"".""AggregateId"" AND l.""OwnerType"" = 'Citation' LIMIT 1) IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditTrailEntries_JurisdictionId_AgencyId_OccurredOnUtc",
                table: "AuditTrailEntries");

            migrationBuilder.DropColumn(
                name: "AgencyId",
                table: "AuditTrailEntries");
        }
    }
}
