using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgencyConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencyConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AgencySequenceCounters",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CounterKey = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    NextValue = table.Column<long>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgencySequenceCounters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrestChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArrestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrestChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ArrestReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArrestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ArrestTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArrestNum = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryIncidentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryMugshotUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrestReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditTrailEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EventType = table.Column<string>(type: "TEXT", nullable: false),
                    Severity = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AggregateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    RecordType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Message = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditTrailEntries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Charges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OffenseName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    UcrCategory = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    NibrsGroup = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CrimeAgainst = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    UcrCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ChargeLevel = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    StateClass = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsCitationEligible = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DefendantNameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsIssued = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CourtId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CitationNum = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Citations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    IsFinalized = table.Column<bool>(type: "INTEGER", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "ABS(RANDOM())"),
                    CitationNum = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CourtId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DefendantNameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsIssued = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Citations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeadLetterMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    OriginalMessageId = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    DeadLetteredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastError = table.Column<string>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    RequeuedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeadLetterMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentArrestLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentRecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    IncidentNum = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ArrestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentArrestLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentArrestLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArrestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentArrestLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentChargeLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChargeId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentChargeLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCitationLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentRecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    IncidentNum = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCitationLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentCitationLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentCitationLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "IncidentReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IncidentNum = table.Column<string>(type: "TEXT", nullable: false),
                    LocalNum = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: ""),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CFSNum = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: ""),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArrestCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CitationCount = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OccurredOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Incidents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "ABS(RANDOM())"),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    OccurredOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IncidentNum = table.Column<string>(type: "TEXT", nullable: false),
                    LocalNum = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: ""),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CFSNum = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, defaultValue: ""),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JurisdictionConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MustCloseAllArrests = table.Column<bool>(type: "INTEGER", nullable: false),
                    MustCloseAllCitations = table.Column<bool>(type: "INTEGER", nullable: false),
                    MustCloseArrestsBeforeIncidentClose = table.Column<bool>(type: "INTEGER", nullable: false),
                    HotkeyNew = table.Column<string>(type: "TEXT", nullable: true),
                    HotkeyModify = table.Column<string>(type: "TEXT", nullable: true),
                    HotkeySave = table.Column<string>(type: "TEXT", nullable: true),
                    HotkeyRelease = table.Column<string>(type: "TEXT", nullable: true),
                    GoogleMapsApiKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JurisdictionConfigurations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LocationReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StreetNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PreDirectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StreetTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PostDirectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CountryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AptSuite = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Coordinates = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CommonPlaceName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "ABS(RANDOM())"),
                    StreetNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PreDirectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    StreetAddress = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    StreetTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PostDirectionId = table.Column<Guid>(type: "TEXT", nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CountryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Zip = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    AptSuite = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Coordinates = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    CommonPlaceName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Comments = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MugshotLinkReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MugshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MugshotLinkReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MugshotLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MugshotId = table.Column<Guid>(type: "TEXT", nullable: false),
                    OwnerType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    OwnerId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsPrimary = table.Column<bool>(type: "INTEGER", nullable: false),
                    DisplayOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    LinkedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LinkedByUserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MugshotLinks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MugshotReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", nullable: false),
                    PublicUrl = table.Column<string>(type: "TEXT", nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MugshotReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mugshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "INTEGER", nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    PublicUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CapturedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mugshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NameReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NameType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RaceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HeightInches = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightLbs = table.Column<int>(type: "INTEGER", nullable: true),
                    HairColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SuffixId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FbiNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LocalNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    PrimaryPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    WorkPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    WorkPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    OtherPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    OtherPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    IsCitizen = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeceasedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsLocked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PrimaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SecondaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryMugshotUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NameReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Names",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NameType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RaceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HeightInches = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightLbs = table.Column<int>(type: "INTEGER", nullable: true),
                    HairColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SuffixId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FbiNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LocalNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    PrimaryPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    WorkPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    WorkPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    OtherPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    OtherPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    IsCitizen = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                    DeceasedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "ABS(RANDOM())"),
                    DeletedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    DeletedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PrimaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SecondaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Names", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AggregateVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Type = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ProcessingStartedOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RetryCount = table.Column<int>(type: "INTEGER", nullable: false),
                    Error = table.Column<string>(type: "TEXT", nullable: true),
                    IsFailedPermanently = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    NextRetryOnUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PicklistItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PicklistType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsSystemDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PicklistItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PicklistSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PicklistType = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IsRequired = table.Column<bool>(type: "INTEGER", nullable: false),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PicklistSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CitationNameSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceNameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceNameRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    LastCopiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCopiedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NameType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RaceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HeightInches = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightLbs = table.Column<int>(type: "INTEGER", nullable: true),
                    HairColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SuffixId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FbiNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LocalNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    PrimaryPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    WorkPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    WorkPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    OtherPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    OtherPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    IsCitizen = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeceasedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PrimaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryLocationRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    PrimaryLocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SecondaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SecondaryLocationRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    SecondaryLocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationNameSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationNameSnapshots_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitationOfficerProfiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceNameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceNameRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    OfficerName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BadgeOrIdentifier = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    UnitNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationOfficerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationOfficerProfiles_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitationTexasDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DocketNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    PageNumber = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    ViolationSourceTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ViolationSection = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ViolationGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryViolationDescription = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    SpeedMph = table.Column<int>(type: "INTEGER", nullable: true),
                    ZoneMph = table.Column<int>(type: "INTEGER", nullable: true),
                    SpeedBandId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NarrativeOtherViolations = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    OccurredAtText = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    CourtAppearanceDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CourtAppearanceLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    AffidavitSignedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ComplainantSignatureText = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    DefendantSignatureText = table.Column<string>(type: "TEXT", maxLength: 150, nullable: true),
                    AcceptedBondNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ReceiptNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationTexasDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationTexasDetails_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CitationVehicles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CitationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PlateNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PlateStateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlateYear = table.Column<int>(type: "INTEGER", nullable: true),
                    ModelYear = table.Column<int>(type: "INTEGER", nullable: true),
                    Make = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Style = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    IsCommercial = table.Column<bool>(type: "INTEGER", nullable: false),
                    CarriesHazardousMaterial = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CitationVehicles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CitationVehicles_Citations_CitationId",
                        column: x => x.CitationId,
                        principalTable: "Citations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Arrests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    NameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ArrestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsFinalized = table.Column<bool>(type: "INTEGER", nullable: false),
                    RecordNumber = table.Column<long>(type: "INTEGER", nullable: false, defaultValueSql: "ABS(RANDOM())"),
                    ArrestNum = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    ArrestTypeId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryIncidentId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    LockedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    LockedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Arrests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Arrests_Incidents_PrimaryIncidentId",
                        column: x => x.PrimaryIncidentId,
                        principalTable: "Incidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Arrests_Names_NameId",
                        column: x => x.NameId,
                        principalTable: "Names",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ArrestNameSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgencyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ArrestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SourceNameId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SourceNameRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    LastCopiedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    LastCopiedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    NameType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RaceId = table.Column<Guid>(type: "TEXT", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "TEXT", nullable: true),
                    HeightInches = table.Column<int>(type: "INTEGER", nullable: true),
                    WeightLbs = table.Column<int>(type: "INTEGER", nullable: true),
                    HairColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SuffixId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    FbiNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    LocalNumber = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    PrimaryPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    WorkPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    WorkPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    OtherPhone = table.Column<string>(type: "TEXT", maxLength: 25, nullable: true),
                    OtherPhoneExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "TEXT", maxLength: 11, nullable: true),
                    IsCitizen = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeceasedDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    PrimaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    PrimaryLocationRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    PrimaryLocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SecondaryLocationId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SecondaryLocationRecordNumber = table.Column<long>(type: "INTEGER", nullable: true),
                    SecondaryLocationAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Version = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArrestNameSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArrestNameSnapshots_Arrests_ArrestId",
                        column: x => x.ArrestId,
                        principalTable: "Arrests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgencyConfigurations_JurisdictionId_AgencyId_Key",
                table: "AgencyConfigurations",
                columns: new[] { "JurisdictionId", "AgencyId", "Key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgencySequenceCounters_JurisdictionId_AgencyId_CounterKey_Year",
                table: "AgencySequenceCounters",
                columns: new[] { "JurisdictionId", "AgencyId", "CounterKey", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ArrestId",
                table: "ArrestChargeLinks",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ArrestId_ChargeId",
                table: "ArrestChargeLinks",
                columns: new[] { "ArrestId", "ChargeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrestChargeLinks_ChargeId",
                table: "ArrestChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestNameSnapshots_ArrestId",
                table: "ArrestNameSnapshots",
                column: "ArrestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_JurisdictionId",
                table: "ArrestReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_NameId",
                table: "ArrestReadModels",
                column: "NameId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_PrimaryIncidentId",
                table: "ArrestReadModels",
                column: "PrimaryIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_ArrestReadModels_Status",
                table: "ArrestReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_NameId",
                table: "Arrests",
                column: "NameId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_PrimaryIncidentId",
                table: "Arrests",
                column: "PrimaryIncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_Arrests_RecordNumber",
                table: "Arrests",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_ActionType",
                table: "AuditTrailEntries",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_EventType",
                table: "AuditTrailEntries",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_JurisdictionId",
                table: "AuditTrailEntries",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_JurisdictionId_OccurredOnUtc",
                table: "AuditTrailEntries",
                columns: new[] { "JurisdictionId", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrailEntries_OccurredOnUtc",
                table: "AuditTrailEntries",
                column: "OccurredOnUtc");

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

            migrationBuilder.CreateIndex(
                name: "IX_Charges_IsActive",
                table: "Charges",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Charges_JurisdictionId_AgencyId_UcrCode_OffenseName",
                table: "Charges",
                columns: new[] { "JurisdictionId", "AgencyId", "UcrCode", "OffenseName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_ChargeId",
                table: "CitationChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_CitationId",
                table: "CitationChargeLinks",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationChargeLinks_CitationId_ChargeId",
                table: "CitationChargeLinks",
                columns: new[] { "CitationId", "ChargeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationNameSnapshots_CitationId",
                table: "CitationNameSnapshots",
                column: "CitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationOfficerProfiles_CitationId",
                table: "CitationOfficerProfiles",
                column: "CitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_IsIssued",
                table: "CitationReadModels",
                column: "IsIssued");

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_JurisdictionId",
                table: "CitationReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_CitationReadModels_UpdatedAtUtc",
                table: "CitationReadModels",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Citations_RecordNumber",
                table: "Citations",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationTexasDetails_CitationId",
                table: "CitationTexasDetails",
                column: "CitationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CitationVehicles_CitationId",
                table: "CitationVehicles",
                column: "CitationId",
                unique: true);

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
                name: "IX_IncidentArrestLinkReadModels_ArrestId",
                table: "IncidentArrestLinkReadModels",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinkReadModels_IncidentId",
                table: "IncidentArrestLinkReadModels",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_ArrestId",
                table: "IncidentArrestLinks",
                column: "ArrestId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_IncidentId",
                table: "IncidentArrestLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentArrestLinks_IncidentId_ArrestId",
                table: "IncidentArrestLinks",
                columns: new[] { "IncidentId", "ArrestId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_ChargeId",
                table: "IncidentChargeLinks",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_IncidentId",
                table: "IncidentChargeLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentChargeLinks_IncidentId_ChargeId",
                table: "IncidentChargeLinks",
                columns: new[] { "IncidentId", "ChargeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinkReadModels_CitationId",
                table: "IncidentCitationLinkReadModels",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinkReadModels_IncidentId",
                table: "IncidentCitationLinkReadModels",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_CitationId",
                table: "IncidentCitationLinks",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_IncidentId",
                table: "IncidentCitationLinks",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentCitationLinks_IncidentId_CitationId",
                table: "IncidentCitationLinks",
                columns: new[] { "IncidentId", "CitationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_JurisdictionId",
                table: "IncidentReadModels",
                column: "JurisdictionId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_Status",
                table: "IncidentReadModels",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentReadModels_UpdatedAtUtc",
                table: "IncidentReadModels",
                column: "UpdatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_JurisdictionId_AgencyId_IncidentNum",
                table: "Incidents",
                columns: new[] { "JurisdictionId", "AgencyId", "IncidentNum" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Incidents_RecordNumber",
                table: "Incidents",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JurisdictionConfigurations_JurisdictionId",
                table: "JurisdictionConfigurations",
                column: "JurisdictionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_City",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_CommonPlaceName",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "CommonPlaceName" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_StateId",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_StreetAddress",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "StreetAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_LocationReadModels_JurisdictionId_Zip",
                table: "LocationReadModels",
                columns: new[] { "JurisdictionId", "Zip" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_City",
                table: "Locations",
                columns: new[] { "JurisdictionId", "City" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_CommonPlaceName",
                table: "Locations",
                columns: new[] { "JurisdictionId", "CommonPlaceName" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_StateId",
                table: "Locations",
                columns: new[] { "JurisdictionId", "StateId" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_StreetAddress",
                table: "Locations",
                columns: new[] { "JurisdictionId", "StreetAddress" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_JurisdictionId_Zip",
                table: "Locations",
                columns: new[] { "JurisdictionId", "Zip" });

            migrationBuilder.CreateIndex(
                name: "IX_Locations_RecordNumber",
                table: "Locations",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MugshotLinkReadModels_JurisdictionId_OwnerType_OwnerId",
                table: "MugshotLinkReadModels",
                columns: new[] { "JurisdictionId", "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_MugshotLinks_MugshotId_OwnerType_OwnerId",
                table: "MugshotLinks",
                columns: new[] { "MugshotId", "OwnerType", "OwnerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MugshotLinks_OwnerType_OwnerId",
                table: "MugshotLinks",
                columns: new[] { "OwnerType", "OwnerId" });

            migrationBuilder.CreateIndex(
                name: "IX_Mugshots_JurisdictionId_CapturedAtUtc",
                table: "Mugshots",
                columns: new[] { "JurisdictionId", "CapturedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_DriversLicenseNumber",
                table: "NameReadModels",
                column: "DriversLicenseNumber");

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_DateOfBirth",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "DateOfBirth" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_DeceasedDate",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "DeceasedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_EyeColorId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "EyeColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_FirstName",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_HairColorId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "HairColorId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_HeightInches",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "HeightInches" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_LastOrBusinessName",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "LastOrBusinessName" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_NameType",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "NameType" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_RaceId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "RaceId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_SexId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "SexId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_SuffixId",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "SuffixId" });

            migrationBuilder.CreateIndex(
                name: "IX_NameReadModels_JurisdictionId_WeightLbs",
                table: "NameReadModels",
                columns: new[] { "JurisdictionId", "WeightLbs" });

            migrationBuilder.CreateIndex(
                name: "IX_Names_RecordNumber",
                table: "Names",
                column: "RecordNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PicklistItems_JurisdictionId_AgencyId_PicklistType_Value",
                table: "PicklistItems",
                columns: new[] { "JurisdictionId", "AgencyId", "PicklistType", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PicklistSettings_JurisdictionId_AgencyId_PicklistType",
                table: "PicklistSettings",
                columns: new[] { "JurisdictionId", "AgencyId", "PicklistType" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgencyConfigurations");

            migrationBuilder.DropTable(
                name: "AgencySequenceCounters");

            migrationBuilder.DropTable(
                name: "ArrestChargeLinks");

            migrationBuilder.DropTable(
                name: "ArrestNameSnapshots");

            migrationBuilder.DropTable(
                name: "ArrestReadModels");

            migrationBuilder.DropTable(
                name: "AuditTrailEntries");

            migrationBuilder.DropTable(
                name: "Charges");

            migrationBuilder.DropTable(
                name: "CitationChargeLinks");

            migrationBuilder.DropTable(
                name: "CitationNameSnapshots");

            migrationBuilder.DropTable(
                name: "CitationOfficerProfiles");

            migrationBuilder.DropTable(
                name: "CitationReadModels");

            migrationBuilder.DropTable(
                name: "CitationTexasDetails");

            migrationBuilder.DropTable(
                name: "CitationVehicles");

            migrationBuilder.DropTable(
                name: "DeadLetterMessages");

            migrationBuilder.DropTable(
                name: "IncidentArrestLinkReadModels");

            migrationBuilder.DropTable(
                name: "IncidentArrestLinks");

            migrationBuilder.DropTable(
                name: "IncidentChargeLinks");

            migrationBuilder.DropTable(
                name: "IncidentCitationLinkReadModels");

            migrationBuilder.DropTable(
                name: "IncidentCitationLinks");

            migrationBuilder.DropTable(
                name: "IncidentReadModels");

            migrationBuilder.DropTable(
                name: "JurisdictionConfigurations");

            migrationBuilder.DropTable(
                name: "LocationReadModels");

            migrationBuilder.DropTable(
                name: "Locations");

            migrationBuilder.DropTable(
                name: "MugshotLinkReadModels");

            migrationBuilder.DropTable(
                name: "MugshotLinks");

            migrationBuilder.DropTable(
                name: "MugshotReadModels");

            migrationBuilder.DropTable(
                name: "Mugshots");

            migrationBuilder.DropTable(
                name: "NameReadModels");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "PicklistItems");

            migrationBuilder.DropTable(
                name: "PicklistSettings");

            migrationBuilder.DropTable(
                name: "Arrests");

            migrationBuilder.DropTable(
                name: "Citations");

            migrationBuilder.DropTable(
                name: "Incidents");

            migrationBuilder.DropTable(
                name: "Names");
        }
    }
}
