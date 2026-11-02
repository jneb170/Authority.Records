using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class ScrubSsnFromEventPayloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Issue #121: NameDetailsUpdated events used to carry the full SSN, so it was stored in
            // plain text in the audit log payload and the outbox. Remove the property from every
            // stored copy. The event no longer has it, so nothing reads it.
            migrationBuilder.Sql(@"UPDATE ""AuditTrailEntries"" SET ""Payload"" = json_remove(""Payload"", '$.SocialSecurityNumber') WHERE ""RecordType"" = 'Name' AND json_valid(""Payload"") AND json_type(""Payload"", '$.SocialSecurityNumber') IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE ""OutboxMessages"" SET ""Content"" = json_remove(""Content"", '$.SocialSecurityNumber') WHERE ""Type"" LIKE '%.NameDetailsUpdatedDomainEvent,%' AND json_valid(""Content"") AND json_type(""Content"", '$.SocialSecurityNumber') IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE ""DeadLetterMessages"" SET ""Content"" = json_remove(""Content"", '$.SocialSecurityNumber') WHERE ""Type"" LIKE '%.NameDetailsUpdatedDomainEvent,%' AND json_valid(""Content"") AND json_type(""Content"", '$.SocialSecurityNumber') IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible by design: the removed SSNs are not restored.
        }
    }
}
