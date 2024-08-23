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
                    IsInactive = table.Column<bool>(type: "INTEGER", nullable: false),
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
                    DayOfWeek = table.Column<int>(type: "INTEGER", nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    RepeatByValue = table.Column<int>(type: "INTEGER", nullable: true),
                    RepeatByType = table.Column<int>(type: "INTEGER", nullable: true),
                    ExcludeDayOfWeek = table.Column<string>(type: "TEXT", nullable: true),
                    ExcludeTimeFrom = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ExcludeTimeTo = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ExcludeTime2From = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    ExcludeTime2To = table.Column<TimeSpan>(type: "TEXT", nullable: true)
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
                    TotalDataAdded = table.Column<int>(type: "INTEGER", nullable: true),
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
                    VerifyModeOrigin = table.Column<int>(type: "INTEGER", nullable: false),
                    VerifyModeView = table.Column<string>(type: "TEXT", nullable: true),
                    InOutModeView = table.Column<string>(type: "TEXT", nullable: true),
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
                    { new Guid("14d2d69d-5dd9-4cce-8d31-6f30ab9be91c"), "ENUM_INOUT_REALAND_IN_KEY", "E_IN" },
                    { new Guid("299c1d42-6320-44a3-9720-89a4e017fa04"), "ENUM_INOUT_REALAND_OUT_KEY", "E_OUT" },
                    { new Guid("8b88f539-f148-40e8-9322-05c2547f8934"), "ENUM_INOUT_ZKTECO_OUT_KEY", "E_OUT" },
                    { new Guid("bfcd5ea4-2e6b-47ea-bf90-06bb6114daac"), "ENUM_INOUT_ZKTECO_IN_KEY", "E_IN" }
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
