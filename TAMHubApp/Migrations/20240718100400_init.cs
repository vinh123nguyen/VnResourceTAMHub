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
                    Value = table.Column<string>(type: "TEXT", nullable: true)
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
                name: "ActionLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimeLogFrom = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeLogTo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateDownDataFrom = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DateDownDataTo = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DownloadType = table.Column<string>(type: "TEXT", nullable: false),
                    DeviceID = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    TotalRowDownload = table.Column<int>(type: "INTEGER", nullable: true),
                    TotalNewlyData = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeRun = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActionLogs_Devices_DeviceID",
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
                table: "AppSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { new Guid("b4df6162-7f2f-40ba-b5bc-e480e07bff5b"), "ENUM_INOUT_ZKTECO_OUT_KEY", "E_OUT" },
                    { new Guid("dc4adbf6-7a31-4a4f-8db5-351e003e4f58"), "ENUM_INOUT_ZKTECO_IN_KEY", "E_IN" }
                });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "AllLogCount", "CommPassword", "CompanyCode", "DeviceSDKType", "IPAddress", "LastTimeConnected", "MachineNo", "Name", "NewLogCount", "Port", "ProductName", "SerialNumber", "ZKTeco_IsCanGetLogByTime", "ZKTeco_IsNewFirmwareMachine", "ZKTeco_IsTFTMachine" },
                values: new object[,]
                {
                    { new Guid("1b9e2472-ca8a-4e90-9a38-34d166560c25"), 140000, 0, null, 0, "10.0.0.13", new DateTime(2024, 7, 18, 16, 4, 0, 498, DateTimeKind.Local).AddTicks(8629), 103, "Máy chấm công 2", null, 4370, null, null, false, null, null },
                    { new Guid("7a0c5e8d-4728-4eaf-ba8e-9fdee23881a4"), 140000, 0, null, 1, "10.20.5.85", new DateTime(2024, 7, 18, 16, 4, 0, 498, DateTimeKind.Local).AddTicks(8632), 103, "Máy chấm công 3", null, 5500, null, null, false, null, null },
                    { new Guid("ea669d45-1a2d-4ac0-bdec-1268ff7a473d"), 140000, 0, null, 0, "10.0.0.231", new DateTime(2024, 7, 18, 16, 4, 0, 498, DateTimeKind.Local).AddTicks(8599), 103, "Máy chấm công 1", null, 4370, null, null, true, null, null }
                });

            migrationBuilder.InsertData(
                table: "ScheduleTasks",
                columns: new[] { "Id", "CycleType", "DayInMonth", "DayOfWeek", "Hour", "LastTimeRun", "Minute", "Name" },
                values: new object[,]
                {
                    { new Guid("868d6504-dcf3-4c04-bfd7-e28c6589da84"), 0, null, null, 10, null, 30, "Schedule 1" },
                    { new Guid("99bf078d-7522-4319-9304-61a9f09f2f73"), 1, null, 1, 10, null, 30, "Schedule 1" },
                    { new Guid("e71d2cc6-8f6d-4560-a3d3-1c8c27ccaa0f"), 2, 15, null, 10, null, 30, "Schedule 1" }
                });

            migrationBuilder.InsertData(
                table: "ActionLogs",
                columns: new[] { "Id", "DateDownDataFrom", "DateDownDataTo", "DeviceID", "DownloadType", "Message", "Status", "TimeLogFrom", "TimeLogTo", "TimeRun", "TotalNewlyData", "TotalRowDownload" },
                values: new object[,]
                {
                    { new Guid("00603b0d-0779-4fd7-8167-472582701370"), new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("ea669d45-1a2d-4ac0-bdec-1268ff7a473d"), "Manual", null, "Success", new DateTime(2024, 7, 18, 17, 4, 0, 498, DateTimeKind.Local).AddTicks(8796), new DateTime(2024, 7, 18, 17, 9, 0, 498, DateTimeKind.Local).AddTicks(8797), new TimeSpan(0, 0, 5, 3, 0), 256, 1000 },
                    { new Guid("9ba91a09-9ba3-4e83-b357-b4513222d30d"), new DateTime(2024, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), new Guid("ea669d45-1a2d-4ac0-bdec-1268ff7a473d"), "Manual", "Error test", "Error", new DateTime(2024, 7, 18, 17, 4, 0, 498, DateTimeKind.Local).AddTicks(8808), new DateTime(2024, 7, 18, 17, 9, 0, 498, DateTimeKind.Local).AddTicks(8809), null, null, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionLogs_DeviceID",
                table: "ActionLogs",
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
                name: "ActionLogs");

            migrationBuilder.DropTable(
                name: "AppSettings");

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
