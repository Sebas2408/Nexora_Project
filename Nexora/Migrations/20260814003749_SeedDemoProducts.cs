using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexora.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Apellido", "ConcurrencyStamp", "Direccion", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "Nombre", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "00000000-aaaa-bbbb-cccc-000000000001", 0, "Vendedor", "490d4bec-b53f-4f3a-a5de-85ae35e509cd", null, "vendor1@example.com", true, false, null, "Demo", "VENDOR1@EXAMPLE.COM", "VENDOR1@EXAMPLE.COM", null, null, false, "", false, "vendor1@example.com" });

            migrationBuilder.InsertData(
                table: "Vendedores",
                columns: new[] { "Id", "Activo", "ApplicationUserId", "Descripcion", "Logo", "NombreTienda" },
                values: new object[] { 1, true, "00000000-aaaa-bbbb-cccc-000000000001", "Tienda de demostración con productos de ejemplo", "/images/tienda-demo.png", "Tienda Demo" });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "Especificaciones", "GarantiaMeses", "ImagenUrl", "Marca", "Modelo", "Nombre", "Precio", "SKU", "Stock", "VendedorId" },
                values: new object[,]
                {
                    { 1, true, 1, "Pantalla Super Retina XDR, 512GB, 48MP cámara, iOS 18.", 24, "https://via.placeholder.com/600x400?text=iPhone+17+Pro+Max", "Apple", "17 Pro Max", "iPhone 17 Pro Max", 1499.99m, "IP17PM-001", 10, 1 },
                    { 2, true, 1, "Pantalla AMOLED, 1TB, 200MP cámara, Android 16.", 24, "https://via.placeholder.com/600x400?text=Samsung+S26+Ultra", "Samsung", "S26 Ultra", "Samsung S26 Ultra", 1399.99m, "SS26U-001", 8, 1 },
                    { 3, true, 1, "Pantalla plegable, 512GB, multitarea avanzada, Android 16.", 24, "https://via.placeholder.com/600x400?text=Samsung+Fold+8", "Samsung", "Fold 8", "Samsung Fold 8", 1899.99m, "SFO8-001", 5, 1 },
                    { 4, true, 1, "Pantalla OLED, 256GB, 48MP cámara, iOS 18.", 24, "https://via.placeholder.com/600x400?text=iPhone+17", "Apple", "17", "iPhone 17", 1099.99m, "IP17-001", 15, 1 },
                    { 5, true, 1, "Pantalla AMOLED, 256GB, 50MP cámara, Android 16.", 24, "https://via.placeholder.com/600x400?text=Samsung+S26", "Samsung", "S26", "Samsung S26", 899.99m, "SS26-001", 20, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Vendedores",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "00000000-aaaa-bbbb-cccc-000000000001");
        }
    }
}
