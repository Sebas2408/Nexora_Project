using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nexora.Migrations
{
    /// <inheritdoc />
    public partial class ModeloActual_2026 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrden_Productos_ProductoId",
                table: "DetallesOrden");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrden_Productos_ProductoId",
                table: "DetallesOrden",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DetallesOrden_Productos_ProductoId",
                table: "DetallesOrden");

            migrationBuilder.AddForeignKey(
                name: "FK_DetallesOrden_Productos_ProductoId",
                table: "DetallesOrden",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
