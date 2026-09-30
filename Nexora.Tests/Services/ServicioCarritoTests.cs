using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.Services;
using Nexora.ViewModels;
using Xunit;

namespace Nexora.Tests.Services;

public sealed class ServicioCarritoTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly ServicioCarrito _servicio;
    private readonly SesionFalsa _sesion = new();

    public ServicioCarritoTests()
    {
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"NexoraCarrito-{Guid.NewGuid()}").Options);
        _servicio = new ServicioCarrito(_db);
    }

    [Fact]
    public async Task Agregar_NoPermiteSuperarLasExistencias()
    {
        await CrearProductoAsync(stock: 2);

        var primeraOperacion = await _servicio.AgregarAsync(_sesion, 1, 2);
        var segundaOperacion = await _servicio.AgregarAsync(_sesion, 1, 1);

        var carrito = await _servicio.ObtenerAsync(_sesion);
        Assert.True(primeraOperacion.Correcto);
        Assert.False(segundaOperacion.Correcto);
        Assert.Single(carrito.Items);
        Assert.Equal(2, carrito.Items[0].Cantidad);
    }

    [Fact]
    public async Task Obtener_ActualizaPrecioYLimitaCantidadAlStockVigente()
    {
        var producto = await CrearProductoAsync(stock: 2, precio: 10m);
        _sesion.SetObject("Cart", new List<CarritoItemViewModel>
        {
            new() { ProductoId = producto.Id, Nombre = "Precio anterior", Precio = 1m, Cantidad = 5 }
        });
        producto.Precio = 25m;
        await _db.SaveChangesAsync();

        var carrito = await _servicio.ObtenerAsync(_sesion);

        Assert.Equal(2, carrito.Items.Single().Cantidad);
        Assert.Equal(25m, carrito.Items.Single().Precio);
        Assert.Equal(50m, carrito.Subtotal);
        Assert.NotEmpty(carrito.Ajustes);
    }

    public void Dispose() => _db.Dispose();

    private async Task<Producto> CrearProductoAsync(int stock, decimal precio = 10m)
    {
        var vendedor = new Vendedor { Id = 1, ApplicationUserId = "seller", NombreTienda = "Tienda", Activo = true };
        var producto = new Producto
        {
            Id = 1, Nombre = "Producto", Marca = "Marca", Especificaciones = "Especificaciones", SKU = $"SKU-{Guid.NewGuid():N}",
            Precio = precio, Stock = stock, GarantiaMeses = 12, CategoriaId = 1, VendedorId = vendedor.Id, Vendedor = vendedor, Activo = true
        };
        _db.Vendedores.Add(vendedor);
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    private sealed class SesionFalsa : ISession
    {
        private readonly Dictionary<string, byte[]> _valores = new();
        public string Id => "test";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _valores.Keys;
        public void Clear() => _valores.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _valores.Remove(key);
        public void Set(string key, byte[] value) => _valores[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _valores.TryGetValue(key, out value!);
    }
}
