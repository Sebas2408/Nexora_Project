using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.Services;
using Nexora.ViewModels;
using System.Data;

namespace Nexora.Controllers;

[Authorize]
public sealed class CartController : Controller
{
    private static readonly HashSet<string> MetodosPagoValidos = new(StringComparer.Ordinal)
    {
        "Tarjeta", "Transferencia", "Contra entrega"
    };

    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ServicioCarrito _carrito;

    public CartController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, ServicioCarrito carrito)
    {
        _db = db;
        _userManager = userManager;
        _carrito = carrito;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var resumen = await _carrito.ObtenerAsync(HttpContext.Session);
        MostrarAjustes(resumen);
        return View(resumen);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
    {
        var resultado = await _carrito.AgregarAsync(HttpContext.Session, productoId, cantidad);
        TempData[resultado.Correcto ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActualizarCantidad(int productoId, int cantidad)
    {
        var resultado = await _carrito.ActualizarCantidadAsync(HttpContext.Session, productoId, cantidad);
        TempData[resultado.Correcto ? "Mensaje" : "Error"] = resultado.Mensaje;
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Quitar(int productoId)
    {
        _carrito.Quitar(HttpContext.Session, productoId);
        TempData["Mensaje"] = "Producto eliminado del carrito.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmarCompra()
    {
        var resumen = await _carrito.ObtenerAsync(HttpContext.Session);
        if (resumen.Items.Count == 0)
        {
            TempData["Error"] = "Agrega al menos un producto antes de finalizar la compra.";
            return RedirectToAction(nameof(Index));
        }

        MostrarAjustes(resumen);
        var usuario = await _userManager.GetUserAsync(User);
        return View(new ConfirmarCompraViewModel
        {
            DireccionEnvio = usuario?.Direccion ?? string.Empty,
            Carrito = resumen
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCompra(ConfirmarCompraViewModel model)
    {
        var resumen = await _carrito.ObtenerAsync(HttpContext.Session);
        model.Carrito = resumen;
        if (!MetodosPagoValidos.Contains(model.MetodoPago))
        {
            ModelState.AddModelError(nameof(model.MetodoPago), "Selecciona un método de pago válido.");
        }
        if (resumen.Items.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Tu carrito está vacío.");
        }
        if (!ModelState.IsValid)
        {
            MostrarAjustes(resumen);
            return View(model);
        }

        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        await using var transaccion = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var ids = resumen.Items.Select(item => item.ProductoId).ToList();
        var productos = await _db.Productos
            .Include(producto => producto.Vendedor)
            .Where(producto => ids.Contains(producto.Id))
            .ToDictionaryAsync(producto => producto.Id);

        foreach (var item in resumen.Items)
        {
            if (!productos.TryGetValue(item.ProductoId, out var producto) ||
                !producto.Activo || producto.Vendedor is not { Activo: true } || producto.Stock < item.Cantidad)
            {
                await transaccion.RollbackAsync();
                TempData["Error"] = $"No hay existencias suficientes para {item.Nombre}. Revisa tu carrito e inténtalo de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }

        var subtotal = resumen.Items.Sum(item => productos[item.ProductoId].Precio * item.Cantidad);
        var impuestos = Math.Round(subtotal * ServicioCarrito.TasaImpuesto, 2, MidpointRounding.AwayFromZero);
        var total = subtotal + ServicioCarrito.CostoEnvio + impuestos;
        var orden = new Orden
        {
            ApplicationUserId = usuario.Id,
            FechaCreacion = DateTime.UtcNow,
            DireccionEnvio = model.DireccionEnvio.Trim(),
            MetodoPago = model.MetodoPago,
            Estado = EstadoEnvio.Pendiente,
            Total = total,
            Detalles = new List<DetalleOrden>()
        };

        foreach (var item in resumen.Items)
        {
            var producto = productos[item.ProductoId];
            producto.Stock -= item.Cantidad;
            orden.Detalles.Add(new DetalleOrden
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        _db.Ordenes.Add(orden);
        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        _carrito.Vaciar(HttpContext.Session);

        return RedirectToAction(nameof(Confirmacion), new { id = orden.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Confirmacion(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var orden = await _db.Ordenes.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.ApplicationUserId == usuario.Id);
        return orden == null ? NotFound() : View(orden);
    }

    private void MostrarAjustes(CarritoResumenViewModel resumen)
    {
        if (resumen.Ajustes.Count > 0)
        {
            TempData["Advertencias"] = string.Join("|", resumen.Ajustes);
        }
    }
}
