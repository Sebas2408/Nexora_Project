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

            // Seed: usuario demo de vendedor, perfil de vendedor y productos de ejemplo (teléfonos)
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
                new Producto
                {
                    Id = 1,
                    Nombre = "iPhone 17 Pro Max",
                    Marca = "Apple",
                    Modelo = "17 Pro Max",
                    Especificaciones = "Pantalla Super Retina XDR, 512GB, 48MP cámara, iOS 18.",
                    SKU = "IP17PM-001",
                    Precio = 1499.99m,
                    Stock = 10,
                    GarantiaMeses = 24,
                    ImagenUrl = "https://via.placeholder.com/600x400?text=iPhone+17+Pro+Max",
                    Activo = true,
                    CategoriaId = 1,
                    VendedorId = 1
                },
                new Producto
                {
                    Id = 2,
                    Nombre = "Samsung S26 Ultra",
                    Marca = "Samsung",
                    Modelo = "S26 Ultra",
                    Especificaciones = "Pantalla AMOLED, 1TB, 200MP cámara, Android 16.",
                    SKU = "SS26U-001",
                    Precio = 1399.99m,
                    Stock = 8,
                    GarantiaMeses = 24,
                    ImagenUrl = "https://via.placeholder.com/600x400?text=Samsung+S26+Ultra",
                    Activo = true,
                    CategoriaId = 1,
                    VendedorId = 1
                },
                new Producto
                {
                    Id = 3,
                    Nombre = "Samsung Fold 8",
                    Marca = "Samsung",
                    Modelo = "Fold 8",
                    Especificaciones = "Pantalla plegable, 512GB, multitarea avanzada, Android 16.",
                    SKU = "SFO8-001",
                    Precio = 1899.99m,
                    Stock = 5,
                    GarantiaMeses = 24,
                    ImagenUrl = "https://via.placeholder.com/600x400?text=Samsung+Fold+8",
                    Activo = true,
                    CategoriaId = 1,
                    VendedorId = 1
                },
                new Producto
                {
                    Id = 4,
                    Nombre = "iPhone 17",
                    Marca = "Apple",
                    Modelo = "17",
                    Especificaciones = "Pantalla OLED, 256GB, 48MP cámara, iOS 18.",
                    SKU = "IP17-001",
                    Precio = 1099.99m,
                    Stock = 15,
                    GarantiaMeses = 24,
                    ImagenUrl = "https://via.placeholder.com/600x400?text=iPhone+17",
                    Activo = true,
                    CategoriaId = 1,
                    VendedorId = 1
                },
                new Producto
                {
                    Id = 5,
                    Nombre = "Samsung S26",
                    Marca = "Samsung",
                    Modelo = "S26",
                    Especificaciones = "Pantalla AMOLED, 256GB, 50MP cámara, Android 16.",
                    SKU = "SS26-001",
                    Precio = 899.99m,
                    Stock = 20,
                    GarantiaMeses = 24,
                    ImagenUrl = "https://via.placeholder.com/600x400?text=Samsung+S26",
                    Activo = true,
                    CategoriaId = 1,
                    VendedorId = 1
                }
            );
        }
    }
}