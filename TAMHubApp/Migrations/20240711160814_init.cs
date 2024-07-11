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
                    CycleType = table.Column<int>(type: "INTEGER", nullable: false),
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
                    { new Guid("039e91c3-6750-4320-be96-6f81f99598c2"), 140000, 0, null, 0, "10.0.0.231", new DateTime(2024, 7, 11, 22, 8, 13, 697, DateTimeKind.Local).AddTicks(2924), 103, "Máy chấm công 1", null, 4370, null, null, true, null, null },
                    { new Guid("5a5dd7bc-556d-4819-8d5c-c8ffb0fd0792"), 140000, 0, null, 1, "10.20.5.85", new DateTime(2024, 7, 11, 22, 8, 13, 697, DateTimeKind.Local).AddTicks(2952), 103, "Máy chấm công 3", null, 5500, null, null, false, null, null },
                    { new Guid("70d4a8b5-5c0e-46c0-b6f7-1d5d2094dfc8"), 140000, 0, null, 0, "10.0.0.13", new DateTime(2024, 7, 11, 22, 8, 13, 697, DateTimeKind.Local).AddTicks(2949), 103, "Máy chấm công 2", null, 4370, null, null, false, null, null }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTasks",
                columns: new[] { "Id", "CycleType", "DayInMonth", "DayOfWeek", "Hour", "LastTimeRun", "Minute", "Name" },
                values: new object[,]
                {
                    { new Guid("167efcd3-bbed-4563-ad25-3cd8332336a5"), 0, null, null, 10, new DateTime(2024, 7, 11, 23, 8, 13, 697, DateTimeKind.Local).AddTicks(3069), 30, "Schedule 1" },
                    { new Guid("7210f2ea-f46b-4814-9df7-958b3b1b3308"), 1, null, 1, 10, new DateTime(2024, 7, 11, 23, 8, 13, 697, DateTimeKind.Local).AddTicks(3073), 30, "Schedule 1" },
                    { new Guid("ec36f76a-6885-48f6-9d77-92d4cf83044b"), 2, 15, null, 10, new DateTime(2024, 7, 11, 23, 8, 13, 697, DateTimeKind.Local).AddTicks(3077), 30, "Schedule 1" }
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
