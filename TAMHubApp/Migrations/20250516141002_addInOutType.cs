using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TAMHubApp.Migrations
{
    /// <inheritdoc />
    public partial class addInOutType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "InOutType",
                table: "Devices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AppSettings",
                columns: new[] { "Id", "Name", "Value" },
                values: new object[,]
                {
                    { new Guid("4f50416e-c25a-4961-bfe9-0968004fb682"), "ENUM_INOUT_OUT_KEY", "E_OUT" },
                    { new Guid("59dcb3e6-b1ef-421f-ad7f-6e8a232538cd"), "ENUM_INOUT_IN_KEY", "E_IN" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("4f50416e-c25a-4961-bfe9-0968004fb682"));

            migrationBuilder.DeleteData(
                table: "AppSettings",
                keyColumn: "Id",
                keyValue: new Guid("59dcb3e6-b1ef-421f-ad7f-6e8a232538cd"));

            migrationBuilder.DropColumn(
                name: "InOutType",
                table: "Devices");

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
    }
}
