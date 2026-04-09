using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shared.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCitationDefendantNameSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DefendantNameId",
                table: "Citations",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefendantNameId",
                table: "CitationReadModels",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CitationNameSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JurisdictionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CitationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SourceNameId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SourceNameRecordNumber = table.Column<long>(type: "bigint", nullable: true),
                    LastCopiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastCopiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NameType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LastOrBusinessName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MiddleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SexId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RaceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DriversLicenseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriversLicenseStateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HeightInches = table.Column<int>(type: "int", nullable: true),
                    WeightLbs = table.Column<int>(type: "int", nullable: true),
                    HairColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EyeColorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SuffixId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FbiNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LocalNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PrimaryPhone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    PrimaryPhoneExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    WorkPhone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    WorkPhoneExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    OtherPhone = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: true),
                    OtherPhoneExtension = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SocialSecurityNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: true),
                    IsCitizen = table.Column<bool>(type: "bit", nullable: false),
                    DeceasedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PrimaryLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PrimaryLocationRecordNumber = table.Column<long>(type: "bigint", nullable: true),
                    PrimaryLocationAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SecondaryLocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SecondaryLocationRecordNumber = table.Column<long>(type: "bigint", nullable: true),
                    SecondaryLocationAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_CitationNameSnapshots_CitationId",
                table: "CitationNameSnapshots",
                column: "CitationId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CitationNameSnapshots");

            migrationBuilder.DropColumn(
                name: "DefendantNameId",
                table: "Citations");

            migrationBuilder.DropColumn(
                name: "DefendantNameId",
                table: "CitationReadModels");
        }
    }
}
