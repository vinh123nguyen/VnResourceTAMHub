using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TAMHubApp.Migrations
{
    /// <inheritdoc />
    public partial class addcoldevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("3ec6369e-dc4b-4783-8b09-b0abe484d609"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("76500526-620e-4108-b1cc-49821ded423a"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("9adb70e5-a674-4c3d-8385-68c072463799"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("f7a8ff7e-859c-4f7a-be88-42ac10c97cf3"));

            migrationBuilder.AddColumn<bool>(
                name: "ZKTeco_IsIFaceDevice",
                table: "Devices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { new Guid("4717dcb8-fcdb-4781-a763-a83d25f453fe"), "ENUM_INOUT_ZKTECO_OUT_KEY", "E_OUT" },
                    { new Guid("850505da-5d09-42bc-922b-db95f741715e"), "ENUM_INOUT_REALAND_IN_KEY", "E_IN" },
                    { new Guid("dd30dcee-4063-4262-bd4b-6bcd4d188ff2"), "ENUM_INOUT_REALAND_OUT_KEY", "E_OUT" },
                    { new Guid("ec4f424e-08f7-4f1e-921f-87f5508fd167"), "ENUM_INOUT_ZKTECO_IN_KEY", "E_IN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("4717dcb8-fcdb-4781-a763-a83d25f453fe"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("850505da-5d09-42bc-922b-db95f741715e"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("dd30dcee-4063-4262-bd4b-6bcd4d188ff2"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("ec4f424e-08f7-4f1e-921f-87f5508fd167"));

            migrationBuilder.DropColumn(
                name: "ZKTeco_IsIFaceDevice",
                table: "Devices");

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { new Guid("3ec6369e-dc4b-4783-8b09-b0abe484d609"), "ENUM_INOUT_ZKTECO_IN_KEY", "E_IN" },
                    { new Guid("76500526-620e-4108-b1cc-49821ded423a"), "ENUM_INOUT_ZKTECO_OUT_KEY", "E_OUT" },
                    { new Guid("9adb70e5-a674-4c3d-8385-68c072463799"), "ENUM_INOUT_REALAND_IN_KEY", "E_IN" },
                    { new Guid("f7a8ff7e-859c-4f7a-be88-42ac10c97cf3"), "ENUM_INOUT_REALAND_OUT_KEY", "E_OUT" }
                });
        }
    }
}
