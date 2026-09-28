using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Controllers;
using Nexora.Data;
using Nexora.Models;
using Xunit;

namespace Nexora.Tests.Controllers;

public sealed class InventarioControllerTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly InventarioController _controller;

    public InventarioControllerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"NexoraTests-{Guid.NewGuid()}")
            .Options;

        _context = new ApplicationDbContext(options);
        _controller = new InventarioController(_context);
    }

    [Fact]
    public async Task Create_Post_GuardaProductoYRedirigeAlInventario()
    {
        var producto = CrearProducto(0, "SKU-ALTA-001", "Producto de prueba");

        var result = await _controller.Create(producto);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventarioController.Index), redirect.ActionName);

        var productoGuardado = await _context.Productos.SingleAsync();
        Assert.Equal("SKU-ALTA-001", productoGuardado.SKU);
        Assert.Equal("Producto de prueba", productoGuardado.Nombre);
        Assert.Equal(129.99m, productoGuardado.Precio);
        Assert.Equal(8, productoGuardado.Stock);
    }

    [Fact]
    public async Task Edit_Post_ActualizaProductoYRedirigeAlInventario()
    {
        var productoExistente = CrearProducto(17, "SKU-EDIT-001", "Nombre anterior");
        _context.Productos.Add(productoExistente);
        await _context.SaveChangesAsync();
        _context.Entry(productoExistente).State = EntityState.Detached;

        var productoActualizado = CrearProducto(17, "SKU-EDIT-001", "Nombre actualizado");
        productoActualizado.Precio = 159.50m;
        productoActualizado.Stock = 12;

        var result = await _controller.Edit(productoActualizado.Id, productoActualizado);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(InventarioController.Index), redirect.ActionName);

        var productoGuardado = await _context.Productos.SingleAsync();
        Assert.Equal("Nombre actualizado", productoGuardado.Nombre);
        Assert.Equal(159.50m, productoGuardado.Precio);
        Assert.Equal(12, productoGuardado.Stock);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static Producto CrearProducto(int id, string sku, string nombre)
    {
        return new Producto
        {
            Id = id,
            Nombre = nombre,
            Marca = "Marca de prueba",
            Modelo = "Modelo de prueba",
            Especificaciones = "Especificaciones de prueba",
            SKU = sku,
            Precio = 129.99m,
            Stock = 8,
            GarantiaMeses = 24,
            CategoriaId = 1,
            VendedorId = 1,
            Activo = true
        };
    }
}
