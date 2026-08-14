using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-aaaa-bbbb-cccc-000000000001",
                column: "ConcurrencyStamp",
                value: "00000000-0000-0000-0000-000000000001");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-aaaa-bbbb-cccc-000000000001",
                column: "ConcurrencyStamp",
                value: "490d4bec-b53f-4f3a-a5de-85ae35e509cd");
        }
    }
}
