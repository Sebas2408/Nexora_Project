using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.ViewModels;

namespace Nexora.Controllers;

[Authorize]
public sealed class PedidosController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public PedidosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var pedidos = await _db.Ordenes.AsNoTracking()
            .Where(orden => orden.ApplicationUserId == usuario.Id)
            .OrderByDescending(orden => orden.FechaCreacion)
            .Select(orden => new PedidoResumenViewModel
            {
                Id = orden.Id,
                FechaCreacion = orden.FechaCreacion,
                Estado = orden.Estado,
                Total = orden.Total,
                CantidadArticulos = orden.Detalles!.Sum(detalle => detalle.Cantidad)
            })
            .ToListAsync();

        return View(pedidos);
    }

    public async Task<IActionResult> Detalle(int id)
    {
        var usuario = await _userManager.GetUserAsync(User);
        if (usuario == null) return Challenge();

        var pedido = await _db.Ordenes.AsNoTracking()
            .Where(orden => orden.Id == id && orden.ApplicationUserId == usuario.Id)
            .Select(orden => new PedidoDetalleViewModel
            {
                Id = orden.Id,
                FechaCreacion = orden.FechaCreacion,
                Estado = orden.Estado,
                DireccionEnvio = orden.DireccionEnvio,
                MetodoPago = orden.MetodoPago,
                Total = orden.Total,
                Lineas = orden.Detalles!.Select(detalle => new PedidoLineaViewModel
                {
                    NombreProducto = detalle.Producto == null ? "Producto no disponible" : detalle.Producto.Nombre,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return pedido == null ? NotFound() : View(pedido);
    }
}
