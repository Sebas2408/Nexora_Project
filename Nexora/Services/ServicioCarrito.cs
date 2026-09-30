using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.ViewModels;

namespace Nexora.Services;

public sealed class ServicioCarrito
{
    public const decimal CostoEnvio = 15m;
    public const decimal TasaImpuesto = 0.08m;
    private const string SessionKeyCart = "Cart";
    private readonly ApplicationDbContext _db;

    public ServicioCarrito(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<CarritoResumenViewModel> ObtenerAsync(ISession session)
    {
        var guardado = session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new();
        if (guardado.Count == 0)
        {
            return CrearResumen(new List<CarritoItemViewModel>());
        }

        var ids = guardado.Select(item => item.ProductoId).Distinct().ToList();
        var productos = await _db.Productos
            .AsNoTracking()
            .Include(producto => producto.Vendedor)
            .Where(producto => ids.Contains(producto.Id))
            .ToDictionaryAsync(producto => producto.Id);

        var actualizados = new List<CarritoItemViewModel>();
        var ajustes = new List<string>();
        foreach (var item in guardado)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto) ||
                !producto.Activo || producto.Vendedor is not { Activo: true } || producto.Stock <= 0)
            {
                ajustes.Add($"{item.Nombre} ya no está disponible y se eliminó del carrito.");
                continue;
            }

            var cantidad = Math.Min(Math.Max(item.Cantidad, 1), producto.Stock);
            if (cantidad != item.Cantidad)
            {
                ajustes.Add($"La cantidad de {producto.Nombre} se ajustó a {cantidad} por disponibilidad.");
            }

            actualizados.Add(new CarritoItemViewModel
            {
                ProductoId = producto.Id,
                Nombre = producto.Nombre,
                ImagenUrl = producto.ImagenUrl,
                Precio = producto.Precio,
                Cantidad = cantidad
            });
        }

        if (!SonIguales(guardado, actualizados))
        {
            session.SetObject(SessionKeyCart, actualizados);
        }

        return CrearResumen(actualizados, ajustes);
    }

    public async Task<ResultadoCarrito> AgregarAsync(ISession session, int productoId, int cantidad)
    {
        if (cantidad < 1)
        {
            return ResultadoCarrito.Error("La cantidad debe ser mayor que cero.");
        }

        var producto = await _db.Productos
            .Include(item => item.Vendedor)
            .FirstOrDefaultAsync(item => item.Id == productoId);

        if (producto == null || !producto.Activo || producto.Vendedor is not { Activo: true })
        {
            return ResultadoCarrito.Error("El producto ya no está disponible.");
        }
        if (producto.Stock == 0)
        {
            return ResultadoCarrito.Error("Este producto no tiene existencias disponibles.");
        }

        var carrito = session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new();
        var existente = carrito.FirstOrDefault(item => item.ProductoId == productoId);
        var nuevaCantidad = (existente?.Cantidad ?? 0) + cantidad;
        if (nuevaCantidad > producto.Stock)
        {
            return ResultadoCarrito.Error($"Solo hay {producto.Stock} unidad(es) disponibles de {producto.Nombre}.");
        }

        if (existente == null)
        {
            carrito.Add(new CarritoItemViewModel { ProductoId = producto.Id, Nombre = producto.Nombre, ImagenUrl = producto.ImagenUrl, Precio = producto.Precio, Cantidad = cantidad });
        }
        else
        {
            existente.Cantidad = nuevaCantidad;
            existente.Nombre = producto.Nombre;
            existente.ImagenUrl = producto.ImagenUrl;
            existente.Precio = producto.Precio;
        }

        session.SetObject(SessionKeyCart, carrito);
        return ResultadoCarrito.Exito("Producto agregado al carrito.");
    }

    public async Task<ResultadoCarrito> ActualizarCantidadAsync(ISession session, int productoId, int cantidad)
    {
        if (cantidad < 1)
        {
            return ResultadoCarrito.Error("Usa la opción de eliminar para quitar un producto del carrito.");
        }

        var producto = await _db.Productos.Include(item => item.Vendedor)
            .FirstOrDefaultAsync(item => item.Id == productoId);
        if (producto == null || !producto.Activo || producto.Vendedor is not { Activo: true })
        {
            return ResultadoCarrito.Error("El producto ya no está disponible.");
        }
        if (cantidad > producto.Stock)
        {
            return ResultadoCarrito.Error($"Solo hay {producto.Stock} unidad(es) disponibles de {producto.Nombre}.");
        }

        var carrito = session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new();
        var item = carrito.FirstOrDefault(value => value.ProductoId == productoId);
        if (item == null)
        {
            return ResultadoCarrito.Error("El producto no está en tu carrito.");
        }

        item.Cantidad = cantidad;
        item.Nombre = producto.Nombre;
        item.ImagenUrl = producto.ImagenUrl;
        item.Precio = producto.Precio;
        session.SetObject(SessionKeyCart, carrito);
        return ResultadoCarrito.Exito("Cantidad actualizada.");
    }

    public void Quitar(ISession session, int productoId)
    {
        var carrito = session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new();
        carrito.RemoveAll(item => item.ProductoId == productoId);
        session.SetObject(SessionKeyCart, carrito);
    }

    public void Vaciar(ISession session) => session.Remove(SessionKeyCart);

    private static CarritoResumenViewModel CrearResumen(List<CarritoItemViewModel> items, IReadOnlyList<string>? ajustes = null)
    {
        var subtotal = items.Sum(item => item.Subtotal);
        return new CarritoResumenViewModel
        {
            Items = items,
            Envio = items.Count == 0 ? 0m : CostoEnvio,
            Impuestos = Math.Round(subtotal * TasaImpuesto, 2, MidpointRounding.AwayFromZero),
            Ajustes = ajustes ?? Array.Empty<string>()
        };
    }

    private static bool SonIguales(IReadOnlyList<CarritoItemViewModel> original, IReadOnlyList<CarritoItemViewModel> actualizado) =>
        original.Count == actualizado.Count && original.Zip(actualizado).All(par =>
            par.First.ProductoId == par.Second.ProductoId &&
            par.First.Cantidad == par.Second.Cantidad &&
            par.First.Precio == par.Second.Precio &&
            par.First.Nombre == par.Second.Nombre &&
            par.First.ImagenUrl == par.Second.ImagenUrl);
}

public sealed record ResultadoCarrito(bool Correcto, string Mensaje)
{
    public static ResultadoCarrito Exito(string mensaje) => new(true, mensaje);
    public static ResultadoCarrito Error(string mensaje) => new(false, mensaje);
}
