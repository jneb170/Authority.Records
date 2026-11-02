using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.SqlServer
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
            migrationBuilder.Sql(@"UPDATE [AuditTrailEntries] SET [Payload] = JSON_MODIFY([Payload], '$.SocialSecurityNumber', NULL) WHERE [RecordType] = 'Name' AND ISJSON([Payload]) = 1 AND JSON_VALUE([Payload], '$.SocialSecurityNumber') IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE [OutboxMessages] SET [Content] = JSON_MODIFY([Content], '$.SocialSecurityNumber', NULL) WHERE [Type] LIKE '%.NameDetailsUpdatedDomainEvent,%' AND ISJSON([Content]) = 1 AND JSON_VALUE([Content], '$.SocialSecurityNumber') IS NOT NULL;");
            migrationBuilder.Sql(@"UPDATE [DeadLetterMessages] SET [Content] = JSON_MODIFY([Content], '$.SocialSecurityNumber', NULL) WHERE [Type] LIKE '%.NameDetailsUpdatedDomainEvent,%' AND ISJSON([Content]) = 1 AND JSON_VALUE([Content], '$.SocialSecurityNumber') IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible by design: the removed SSNs are not restored.
        }
    }
}
