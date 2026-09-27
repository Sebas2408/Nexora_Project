using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nexora.Models;
using System.Reflection.Emit;

namespace Nexora.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Vendedor> Vendedores { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<DetalleOrden> DetallesOrden { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SKU único
            builder.Entity<Producto>()
                .HasIndex(p => p.SKU)
                .IsUnique();
            // Configurar comportamiento de borrado para evitar múltiples rutas de cascade en SQL Server
            // Evitamos cascade delete desde Producto -> DetalleOrden para prevenir el error
            builder.Entity<DetalleOrden>()
                .HasOne(d => d.Orden)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<DetalleOrden>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed de categorías
            builder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Celulares", Icono = "bi-phone" },
                new Categoria { Id = 2, Nombre = "Laptops", Icono = "bi-laptop" },
                new Categoria { Id = 3, Nombre = "Tablets", Icono = "bi-tablet" },
                new Categoria { Id = 4, Nombre = "Audio", Icono = "bi-headphones" },
                new Categoria { Id = 5, Nombre = "Gaming", Icono = "bi-controller" },
                new Categoria { Id = 6, Nombre = "Accesorios", Icono = "bi-plug" }
            );

            // Seed: usuario demo de vendedor, perfil de vendedor y catálogo inicial
            // Nota: estos datos se usan para desarrollo local y aparecen en el catálogo
            var demoUserId = "00000000-aaaa-bbbb-cccc-000000000001";

            builder.Entity<ApplicationUser>().HasData(new ApplicationUser
            {
                Id = demoUserId,
                Nombre = "Demo",
                Apellido = "Vendedor",
                UserName = "vendor1@example.com",
                NormalizedUserName = "VENDOR1@EXAMPLE.COM",
                Email = "vendor1@example.com",
                NormalizedEmail = "VENDOR1@EXAMPLE.COM",
                EmailConfirmed = true,
                SecurityStamp = "",
                ConcurrencyStamp = "00000000-0000-0000-0000-000000000001",
                AccessFailedCount = 0,
                LockoutEnabled = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false
            });

            builder.Entity<Vendedor>().HasData(new Vendedor
            {
                Id = 1,
                NombreTienda = "Tienda Demo",
                Descripcion = "Tienda de demostración con productos de ejemplo",
                Logo = "/images/tienda-demo.png",
                ApplicationUserId = demoUserId,
                Activo = true
            });

            builder.Entity<Producto>().HasData(
                ProductoSeed(1, "iPhone 17 Pro Max", "Apple", "17 Pro Max", 1499.99m, 1, "IP17PM-001"),
                ProductoSeed(2, "Samsung Galaxy S26 Ultra", "Samsung", "Galaxy S26 Ultra", 1399.99m, 1, "SS26U-001"),
                ProductoSeed(3, "Samsung Galaxy Fold 8", "Samsung", "Galaxy Fold 8", 1899.99m, 1, "SFO8-001"),
                ProductoSeed(4, "iPhone 17", "Apple", "17", 1099.99m, 1, "IP17-001"),
                ProductoSeed(5, "Samsung Galaxy S26", "Samsung", "Galaxy S26", 899.99m, 1, "SS26-001"),
                ProductoSeed(6, "Google Pixel 10 Pro", "Google", "Pixel 10 Pro", 999.99m, 1, "GP10P-001"),
                ProductoSeed(7, "Xiaomi 15 Ultra", "Xiaomi", "15 Ultra", 849.99m, 1, "X15U-001"),
                ProductoSeed(8, "Motorola Edge 60", "Motorola", "Edge 60", 549.99m, 1, "ME60-001"),
                ProductoSeed(9, "MacBook Air M4", "Apple", "Air M4", 1199.99m, 2, "MBA-M4-001"),
                ProductoSeed(10, "MacBook Pro 16\" M4 Max", "Apple", "Pro 16 M4 Max", 2899.99m, 2, "MBP16-M4M-001"),
                ProductoSeed(11, "Dell XPS 15", "Dell", "XPS 15", 1599.99m, 2, "DXPS15-001"),
                ProductoSeed(12, "Lenovo ThinkPad X1 Carbon", "Lenovo", "ThinkPad X1 Carbon", 1749.99m, 2, "TPX1C-001"),
                ProductoSeed(13, "ASUS ROG Zephyrus G16", "Asus", "ROG Zephyrus G16", 2199.99m, 2, "ROGG16-001"),
                ProductoSeed(14, "HP Spectre x360", "HP", "Spectre x360", 1449.99m, 2, "HPSX360-001"),
                ProductoSeed(15, "iPad Pro 13\" M4", "Apple", "iPad Pro 13 M4", 1299.99m, 3, "IPADPRO13-001"),
                ProductoSeed(16, "iPad Air 11\"", "Apple", "iPad Air 11", 699.99m, 3, "IPADAIR11-001"),
                ProductoSeed(17, "Samsung Galaxy Tab S10 Ultra", "Samsung", "Galaxy Tab S10 Ultra", 1199.99m, 3, "SGTABS10U-001"),
                ProductoSeed(18, "Microsoft Surface Pro 11", "Microsoft", "Surface Pro 11", 1099.99m, 3, "MSPRO11-001"),
                ProductoSeed(19, "Lenovo Tab P12", "Lenovo", "Tab P12", 499.99m, 3, "LTP12-001"),
                ProductoSeed(20, "AirPods Pro 3", "Apple", "Pro 3", 249.99m, 4, "APPRO3-001"),
                ProductoSeed(21, "Sony WH-1000XM6", "Sony", "WH-1000XM6", 399.99m, 4, "SWH1000XM6-001"),
                ProductoSeed(22, "Bose QuietComfort Ultra", "Bose", "QuietComfort Ultra", 379.99m, 4, "BQCULTRA-001"),
                ProductoSeed(23, "JBL Charge 6", "JBL", "Charge 6", 199.99m, 4, "JBLCH6-001"),
                ProductoSeed(24, "Samsung Galaxy Buds 4 Pro", "Samsung", "Galaxy Buds 4 Pro", 229.99m, 4, "SGBUDS4P-001"),
                ProductoSeed(25, "Sonos Era 300", "Sonos", "Era 300", 449.99m, 4, "SONOSERA300-001"),
                ProductoSeed(26, "PlayStation 5 Pro", "Sony", "PS5 Pro", 699.99m, 5, "PS5PRO-001"),
                ProductoSeed(27, "Xbox Series X", "Microsoft", "Series X", 499.99m, 5, "XBOXSX-001"),
                ProductoSeed(28, "Nintendo Switch 2", "Nintendo", "Switch 2", 449.99m, 5, "NSW2-001"),
                ProductoSeed(29, "Logitech G Pro X Superlight 2", "Logitech", "G Pro X Superlight 2", 159.99m, 5, "LGPSL2-001"),
                ProductoSeed(30, "Razer BlackWidow V4", "Razer", "BlackWidow V4", 189.99m, 5, "RBWV4-001"),
                ProductoSeed(31, "ASUS ROG Ally X", "Asus", "ROG Ally X", 799.99m, 5, "ROGALLYX-001"),
                ProductoSeed(32, "Cargador MagSafe 2 en 1", "Apple", "MagSafe 2 en 1", 79.99m, 6, "MAGSAFE21-001"),
                ProductoSeed(33, "Power bank Anker 20,000mAh", "Anker", "Power bank 20,000mAh", 59.99m, 6, "ANKPB20K-001"),
                ProductoSeed(34, "Funda protectora iPhone 17 Pro", "Genérica", "Funda iPhone 17 Pro", 39.99m, 6, "FUNDAIP17P-001"),
                ProductoSeed(35, "Mouse inalámbrico Logitech MX Master 4", "Logitech", "MX Master 4", 109.99m, 6, "MXM4-001"),
                ProductoSeed(36, "Hub USB-C 7 en 1", "Genérica", "Hub USB-C 7 en 1", 49.99m, 6, "HUBUSBC7-001"),
                ProductoSeed(37, "Soporte ajustable para laptop", "Genérica", "Soporte para laptop", 34.99m, 6, "SOPLAP-001")
            );
        }

        private static Producto ProductoSeed(int id, string nombre, string marca, string modelo, decimal precio, int categoriaId, string sku)
        {
            return new Producto
            {
                Id = id,
                Nombre = nombre,
                Marca = marca,
                Modelo = modelo,
                Especificaciones = $"Producto {nombre} de {marca}.",
                SKU = sku,
                Precio = precio,
                Rating = 4.8m,
                Stock = 10,
                GarantiaMeses = 24,
                ImagenUrl = $"https://via.placeholder.com/600x400?text={Uri.EscapeDataString(nombre)}",
                Activo = true,
                CategoriaId = categoriaId,
                VendedorId = 1
            };
        }
    }
}