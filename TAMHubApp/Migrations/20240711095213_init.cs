using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

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
                    CommPassword = table.Column<int>(type: "INTEGER", nullable: true),
                    DeviceSDKType = table.Column<int>(type: "INTEGER", nullable: false),
                    AllLogCount = table.Column<int>(type: "INTEGER", nullable: true),
                    NewLogCount = table.Column<int>(type: "INTEGER", nullable: true),
                    LastTimeConnected = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SerialNumber = table.Column<string>(type: "TEXT", nullable: true),
                    ProductName = table.Column<string>(type: "TEXT", nullable: true),
                    CompanyCode = table.Column<string>(type: "TEXT", nullable: true),
                    ZKTeco_IsCanGetLogByTime = table.Column<bool>(type: "INTEGER", nullable: false),
                    ZKTeco_IsTFTMachine = table.Column<bool>(type: "INTEGER", nullable: true),
                    ZKTeco_IsNewFirmwareMachine = table.Column<bool>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleTasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    LastTimeRun = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CycleType = table.Column<string>(type: "TEXT", nullable: false),
                    Hour = table.Column<int>(type: "INTEGER", nullable: false),
                    Minute = table.Column<int>(type: "INTEGER", nullable: false),
                    DayInMonth = table.Column<int>(type: "INTEGER", nullable: true),
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleTasks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DeviceActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActionName = table.Column<string>(type: "TEXT", nullable: false),
                    ActionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DeviceID = table.Column<Guid>(type: "TEXT", nullable: true),
                    ResultStatus = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<string>(type: "TEXT", nullable: false),
                    ActionType = table.Column<string>(type: "TEXT", nullable: false)
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
                    InOutModeView = table.Column<string>(type: "TEXT", nullable: false),
                    InOutModeOrigin = table.Column<string>(type: "TEXT", nullable: false),
                    TimeLog = table.Column<DateTime>(type: "TEXT", nullable: false)
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
                    InOutModeView = table.Column<string>(type: "TEXT", nullable: false),
                    InOutModeOrigin = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeLog = table.Column<DateTime>(type: "TEXT", nullable: false)
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

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "AllLogCount", "CommPassword", "CompanyCode", "DeviceSDKType", "IPAddress", "LastTimeConnected", "MachineNo", "Name", "NewLogCount", "Port", "ProductName", "SerialNumber", "ZKTeco_IsCanGetLogByTime", "ZKTeco_IsNewFirmwareMachine", "ZKTeco_IsTFTMachine" },
                values: new object[,]
                {
                    { new Guid("7e549b66-0f1f-4e10-9f89-6b9d48032c84"), 140000, 0, null, 0, "10.0.0.231", new DateTime(2024, 7, 11, 15, 52, 12, 974, DateTimeKind.Local).AddTicks(3490), 103, "Máy chấm công 1", null, 4370, null, null, true, null, null },
                    { new Guid("bf2bf4a8-6178-43cf-b5eb-f24fdabb8982"), 140000, 0, null, 0, "10.0.0.13", new DateTime(2024, 7, 11, 15, 52, 12, 974, DateTimeKind.Local).AddTicks(3509), 103, "Máy chấm công 2", null, 4370, null, null, false, null, null },
                    { new Guid("cf7106ff-e6d2-4203-9b80-fcad23e4e2cb"), 140000, 0, null, 1, "10.20.5.85", new DateTime(2024, 7, 11, 15, 52, 12, 974, DateTimeKind.Local).AddTicks(3512), 103, "Máy chấm công 2", null, 5500, null, null, false, null, null }
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
                name: "ScheduleTasks");

            migrationBuilder.DropTable(
                name: "TAMScanLogHistories");

            migrationBuilder.DropTable(
                name: "TAMScanLogs");

            migrationBuilder.DropTable(
                name: "Devices");
        }
    }
}
