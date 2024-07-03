using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TAMHubApp.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    MachineNo = table.Column<int>(type: "INTEGER", nullable: false),
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Port = table.Column<int>(type: "INTEGER", nullable: false),
                    CommPassword = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionName = table.Column<string>(type: "TEXT", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeviceID = table.Column<Guid>(type: "TEXT", nullable: false),
                    ResultStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeviceActionLogs_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TAMScanLogHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceID = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnrollNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VerifyMode = table.Column<string>(type: "TEXT", nullable: false),
                    InOutMode = table.Column<string>(type: "TEXT", nullable: false),
                    TimeLog = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAMScanLogHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAMScanLogHistories_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TAMScanLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DeviceID = table.Column<Guid>(type: "TEXT", nullable: false),
                    EnrollNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VerifyMode = table.Column<string>(type: "TEXT", nullable: false),
                    InOutMode = table.Column<string>(type: "TEXT", nullable: false),
                    TimeLog = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAMScanLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAMScanLogs_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceActionLogs_DeviceID",
                table: "DeviceActionLogs",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_TAMScanLogHistories_DeviceID",
                table: "TAMScanLogHistories",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_TAMScanLogs_DeviceID",
                table: "TAMScanLogs",
                column: "DeviceID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.DropTable(
                name: "DeviceActionLogs");

            migrationBuilder.DropTable(
                name: "TAMScanLogHistories");

            migrationBuilder.DropTable(
                name: "TAMScanLogs");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
