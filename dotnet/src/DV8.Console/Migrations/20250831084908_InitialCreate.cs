using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DV8.Console.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CcpDecisions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Context = table.Column<string>(type: "TEXT", nullable: true),
                    Decision = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LedgerHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CcpDecisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DbgSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DbgHash = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Geo = table.Column<string>(type: "TEXT", nullable: true),
                    RiskFloor = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DbgSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tenant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kind = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vendor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SwVersion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fingerprint = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    TrustState = table.Column<string>(type: "TEXT", nullable: true),
                    Site = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Posture = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DiscoveryJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tenant = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassiveOnly = table.Column<bool>(type: "bit", nullable: false),
                    Cidrs = table.Column<string>(type: "TEXT", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DbgSession = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoveryJobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoacEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Profile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Risk = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Transcript = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoacEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoacEvents_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Interfaces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mac = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ipv4 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ipv6 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SpeedMbps = table.Column<int>(type: "int", nullable: true),
                    Up = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interfaces", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interfaces_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SecretFields",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ciphertext = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Iv = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Aad = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    LedgerPtr = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecretFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecretFields_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "DiscoveryResults",
                columns: table => new
                {
                    JobId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Fingerprint = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Addrs = table.Column<string>(type: "TEXT", nullable: true),
                    Signals = table.Column<string>(type: "TEXT", nullable: true),
                    Confidence = table.Column<float>(type: "real", nullable: true),
                    Raw = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscoveryResults", x => new { x.JobId, x.Fingerprint });
                    table.ForeignKey(
                        name: "FK_DiscoveryResults_DiscoveryJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "DiscoveryJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Neighbors",
                columns: table => new
                {
                    AIfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZIfId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Proto = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Confidence = table.Column<float>(type: "real", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Neighbors", x => new { x.AIfId, x.ZIfId, x.Proto });
                    table.ForeignKey(
                        name: "FK_Neighbors_Interfaces_AIfId",
                        column: x => x.AIfId,
                        principalTable: "Interfaces",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Neighbors_Interfaces_ZIfId",
                        column: x => x.ZIfId,
                        principalTable: "Interfaces",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Devices_Fingerprint",
                table: "Devices",
                column: "Fingerprint",
                unique: true,
                filter: "[Fingerprint] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HoacEvents_DeviceId",
                table: "HoacEvents",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Interfaces_DeviceId",
                table: "Interfaces",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_Neighbors_ZIfId",
                table: "Neighbors",
                column: "ZIfId");

            migrationBuilder.CreateIndex(
                name: "IX_SecretFields_DeviceId",
                table: "SecretFields",
                column: "DeviceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CcpDecisions");

            migrationBuilder.DropTable(
                name: "DbgSessions");

            migrationBuilder.DropTable(
                name: "DiscoveryResults");

            migrationBuilder.DropTable(
                name: "HoacEvents");

            migrationBuilder.DropTable(
                name: "Neighbors");

            migrationBuilder.DropTable(
                name: "SecretFields");

            migrationBuilder.DropTable(
                name: "DiscoveryJobs");

            migrationBuilder.DropTable(
                name: "Interfaces");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
