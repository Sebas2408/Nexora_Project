using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Nexora.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Rating",
                table: "Productos",
                type: "decimal(2,1)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Especificaciones", "ImagenUrl", "Rating" },
                values: new object[] { "Producto iPhone 17 Pro Max de Apple.", "https://via.placeholder.com/600x400?text=iPhone%2017%20Pro%20Max", 4.8m });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Rating", "Stock" },
                values: new object[] { "Producto Samsung Galaxy S26 Ultra de Samsung.", "https://via.placeholder.com/600x400?text=Samsung%20Galaxy%20S26%20Ultra", "Galaxy S26 Ultra", "Samsung Galaxy S26 Ultra", 4.8m, 10 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Rating", "Stock" },
                values: new object[] { "Producto Samsung Galaxy Fold 8 de Samsung.", "https://via.placeholder.com/600x400?text=Samsung%20Galaxy%20Fold%208", "Galaxy Fold 8", "Samsung Galaxy Fold 8", 4.8m, 10 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Especificaciones", "ImagenUrl", "Rating", "Stock" },
                values: new object[] { "Producto iPhone 17 de Apple.", "https://via.placeholder.com/600x400?text=iPhone%2017", 4.8m, 10 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Rating", "Stock" },
                values: new object[] { "Producto Samsung Galaxy S26 de Samsung.", "https://via.placeholder.com/600x400?text=Samsung%20Galaxy%20S26", "Galaxy S26", "Samsung Galaxy S26", 4.8m, 10 });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "CategoriaId", "Especificaciones", "GarantiaMeses", "ImagenUrl", "Marca", "Modelo", "Nombre", "Precio", "Rating", "SKU", "Stock", "VendedorId" },
                values: new object[,]
                {
                    { 6, true, 1, "Producto Google Pixel 10 Pro de Google.", 24, "https://via.placeholder.com/600x400?text=Google%20Pixel%2010%20Pro", "Google", "Pixel 10 Pro", "Google Pixel 10 Pro", 999.99m, 4.8m, "GP10P-001", 10, 1 },
                    { 7, true, 1, "Producto Xiaomi 15 Ultra de Xiaomi.", 24, "https://via.placeholder.com/600x400?text=Xiaomi%2015%20Ultra", "Xiaomi", "15 Ultra", "Xiaomi 15 Ultra", 849.99m, 4.8m, "X15U-001", 10, 1 },
                    { 8, true, 1, "Producto Motorola Edge 60 de Motorola.", 24, "https://via.placeholder.com/600x400?text=Motorola%20Edge%2060", "Motorola", "Edge 60", "Motorola Edge 60", 549.99m, 4.8m, "ME60-001", 10, 1 },
                    { 9, true, 2, "Producto MacBook Air M4 de Apple.", 24, "https://via.placeholder.com/600x400?text=MacBook%20Air%20M4", "Apple", "Air M4", "MacBook Air M4", 1199.99m, 4.8m, "MBA-M4-001", 10, 1 },
                    { 10, true, 2, "Producto MacBook Pro 16\" M4 Max de Apple.", 24, "https://via.placeholder.com/600x400?text=MacBook%20Pro%2016%22%20M4%20Max", "Apple", "Pro 16 M4 Max", "MacBook Pro 16\" M4 Max", 2899.99m, 4.8m, "MBP16-M4M-001", 10, 1 },
                    { 11, true, 2, "Producto Dell XPS 15 de Dell.", 24, "https://via.placeholder.com/600x400?text=Dell%20XPS%2015", "Dell", "XPS 15", "Dell XPS 15", 1599.99m, 4.8m, "DXPS15-001", 10, 1 },
                    { 12, true, 2, "Producto Lenovo ThinkPad X1 Carbon de Lenovo.", 24, "https://via.placeholder.com/600x400?text=Lenovo%20ThinkPad%20X1%20Carbon", "Lenovo", "ThinkPad X1 Carbon", "Lenovo ThinkPad X1 Carbon", 1749.99m, 4.8m, "TPX1C-001", 10, 1 },
                    { 13, true, 2, "Producto ASUS ROG Zephyrus G16 de Asus.", 24, "https://via.placeholder.com/600x400?text=ASUS%20ROG%20Zephyrus%20G16", "Asus", "ROG Zephyrus G16", "ASUS ROG Zephyrus G16", 2199.99m, 4.8m, "ROGG16-001", 10, 1 },
                    { 14, true, 2, "Producto HP Spectre x360 de HP.", 24, "https://via.placeholder.com/600x400?text=HP%20Spectre%20x360", "HP", "Spectre x360", "HP Spectre x360", 1449.99m, 4.8m, "HPSX360-001", 10, 1 },
                    { 15, true, 3, "Producto iPad Pro 13\" M4 de Apple.", 24, "https://via.placeholder.com/600x400?text=iPad%20Pro%2013%22%20M4", "Apple", "iPad Pro 13 M4", "iPad Pro 13\" M4", 1299.99m, 4.8m, "IPADPRO13-001", 10, 1 },
                    { 16, true, 3, "Producto iPad Air 11\" de Apple.", 24, "https://via.placeholder.com/600x400?text=iPad%20Air%2011%22", "Apple", "iPad Air 11", "iPad Air 11\"", 699.99m, 4.8m, "IPADAIR11-001", 10, 1 },
                    { 17, true, 3, "Producto Samsung Galaxy Tab S10 Ultra de Samsung.", 24, "https://via.placeholder.com/600x400?text=Samsung%20Galaxy%20Tab%20S10%20Ultra", "Samsung", "Galaxy Tab S10 Ultra", "Samsung Galaxy Tab S10 Ultra", 1199.99m, 4.8m, "SGTABS10U-001", 10, 1 },
                    { 18, true, 3, "Producto Microsoft Surface Pro 11 de Microsoft.", 24, "https://via.placeholder.com/600x400?text=Microsoft%20Surface%20Pro%2011", "Microsoft", "Surface Pro 11", "Microsoft Surface Pro 11", 1099.99m, 4.8m, "MSPRO11-001", 10, 1 },
                    { 19, true, 3, "Producto Lenovo Tab P12 de Lenovo.", 24, "https://via.placeholder.com/600x400?text=Lenovo%20Tab%20P12", "Lenovo", "Tab P12", "Lenovo Tab P12", 499.99m, 4.8m, "LTP12-001", 10, 1 },
                    { 20, true, 4, "Producto AirPods Pro 3 de Apple.", 24, "https://via.placeholder.com/600x400?text=AirPods%20Pro%203", "Apple", "Pro 3", "AirPods Pro 3", 249.99m, 4.8m, "APPRO3-001", 10, 1 },
                    { 21, true, 4, "Producto Sony WH-1000XM6 de Sony.", 24, "https://via.placeholder.com/600x400?text=Sony%20WH-1000XM6", "Sony", "WH-1000XM6", "Sony WH-1000XM6", 399.99m, 4.8m, "SWH1000XM6-001", 10, 1 },
                    { 22, true, 4, "Producto Bose QuietComfort Ultra de Bose.", 24, "https://via.placeholder.com/600x400?text=Bose%20QuietComfort%20Ultra", "Bose", "QuietComfort Ultra", "Bose QuietComfort Ultra", 379.99m, 4.8m, "BQCULTRA-001", 10, 1 },
                    { 23, true, 4, "Producto JBL Charge 6 de JBL.", 24, "https://via.placeholder.com/600x400?text=JBL%20Charge%206", "JBL", "Charge 6", "JBL Charge 6", 199.99m, 4.8m, "JBLCH6-001", 10, 1 },
                    { 24, true, 4, "Producto Samsung Galaxy Buds 4 Pro de Samsung.", 24, "https://via.placeholder.com/600x400?text=Samsung%20Galaxy%20Buds%204%20Pro", "Samsung", "Galaxy Buds 4 Pro", "Samsung Galaxy Buds 4 Pro", 229.99m, 4.8m, "SGBUDS4P-001", 10, 1 },
                    { 25, true, 4, "Producto Sonos Era 300 de Sonos.", 24, "https://via.placeholder.com/600x400?text=Sonos%20Era%20300", "Sonos", "Era 300", "Sonos Era 300", 449.99m, 4.8m, "SONOSERA300-001", 10, 1 },
                    { 26, true, 5, "Producto PlayStation 5 Pro de Sony.", 24, "https://via.placeholder.com/600x400?text=PlayStation%205%20Pro", "Sony", "PS5 Pro", "PlayStation 5 Pro", 699.99m, 4.8m, "PS5PRO-001", 10, 1 },
                    { 27, true, 5, "Producto Xbox Series X de Microsoft.", 24, "https://via.placeholder.com/600x400?text=Xbox%20Series%20X", "Microsoft", "Series X", "Xbox Series X", 499.99m, 4.8m, "XBOXSX-001", 10, 1 },
                    { 28, true, 5, "Producto Nintendo Switch 2 de Nintendo.", 24, "https://via.placeholder.com/600x400?text=Nintendo%20Switch%202", "Nintendo", "Switch 2", "Nintendo Switch 2", 449.99m, 4.8m, "NSW2-001", 10, 1 },
                    { 29, true, 5, "Producto Logitech G Pro X Superlight 2 de Logitech.", 24, "https://via.placeholder.com/600x400?text=Logitech%20G%20Pro%20X%20Superlight%202", "Logitech", "G Pro X Superlight 2", "Logitech G Pro X Superlight 2", 159.99m, 4.8m, "LGPSL2-001", 10, 1 },
                    { 30, true, 5, "Producto Razer BlackWidow V4 de Razer.", 24, "https://via.placeholder.com/600x400?text=Razer%20BlackWidow%20V4", "Razer", "BlackWidow V4", "Razer BlackWidow V4", 189.99m, 4.8m, "RBWV4-001", 10, 1 },
                    { 31, true, 5, "Producto ASUS ROG Ally X de Asus.", 24, "https://via.placeholder.com/600x400?text=ASUS%20ROG%20Ally%20X", "Asus", "ROG Ally X", "ASUS ROG Ally X", 799.99m, 4.8m, "ROGALLYX-001", 10, 1 },
                    { 32, true, 6, "Producto Cargador MagSafe 2 en 1 de Apple.", 24, "https://via.placeholder.com/600x400?text=Cargador%20MagSafe%202%20en%201", "Apple", "MagSafe 2 en 1", "Cargador MagSafe 2 en 1", 79.99m, 4.8m, "MAGSAFE21-001", 10, 1 },
                    { 33, true, 6, "Producto Power bank Anker 20,000mAh de Anker.", 24, "https://via.placeholder.com/600x400?text=Power%20bank%20Anker%2020%2C000mAh", "Anker", "Power bank 20,000mAh", "Power bank Anker 20,000mAh", 59.99m, 4.8m, "ANKPB20K-001", 10, 1 },
                    { 34, true, 6, "Producto Funda protectora iPhone 17 Pro de Genérica.", 24, "https://via.placeholder.com/600x400?text=Funda%20protectora%20iPhone%2017%20Pro", "Genérica", "Funda iPhone 17 Pro", "Funda protectora iPhone 17 Pro", 39.99m, 4.8m, "FUNDAIP17P-001", 10, 1 },
                    { 35, true, 6, "Producto Mouse inalámbrico Logitech MX Master 4 de Logitech.", 24, "https://via.placeholder.com/600x400?text=Mouse%20inal%C3%A1mbrico%20Logitech%20MX%20Master%204", "Logitech", "MX Master 4", "Mouse inalámbrico Logitech MX Master 4", 109.99m, 4.8m, "MXM4-001", 10, 1 },
                    { 36, true, 6, "Producto Hub USB-C 7 en 1 de Genérica.", 24, "https://via.placeholder.com/600x400?text=Hub%20USB-C%207%20en%201", "Genérica", "Hub USB-C 7 en 1", "Hub USB-C 7 en 1", 49.99m, 4.8m, "HUBUSBC7-001", 10, 1 },
                    { 37, true, 6, "Producto Soporte ajustable para laptop de Genérica.", 24, "https://via.placeholder.com/600x400?text=Soporte%20ajustable%20para%20laptop", "Genérica", "Soporte para laptop", "Soporte ajustable para laptop", 34.99m, 4.8m, "SOPLAP-001", 10, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DropColumn(
                name: "Rating",
                table: "Productos");

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Especificaciones", "ImagenUrl" },
                values: new object[] { "Pantalla Super Retina XDR, 512GB, 48MP cámara, iOS 18.", "https://via.placeholder.com/600x400?text=iPhone+17+Pro+Max" });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Stock" },
                values: new object[] { "Pantalla AMOLED, 1TB, 200MP cámara, Android 16.", "https://via.placeholder.com/600x400?text=Samsung+S26+Ultra", "S26 Ultra", "Samsung S26 Ultra", 8 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Stock" },
                values: new object[] { "Pantalla plegable, 512GB, multitarea avanzada, Android 16.", "https://via.placeholder.com/600x400?text=Samsung+Fold+8", "Fold 8", "Samsung Fold 8", 5 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Especificaciones", "ImagenUrl", "Stock" },
                values: new object[] { "Pantalla OLED, 256GB, 48MP cámara, iOS 18.", "https://via.placeholder.com/600x400?text=iPhone+17", 15 });

            migrationBuilder.UpdateData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Especificaciones", "ImagenUrl", "Modelo", "Nombre", "Stock" },
                values: new object[] { "Pantalla AMOLED, 256GB, 50MP cámara, Android 16.", "https://via.placeholder.com/600x400?text=Samsung+S26", "S26", "Samsung S26", 20 });
        }
    }
}
