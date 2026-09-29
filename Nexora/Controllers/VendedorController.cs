using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.Services;

namespace Nexora.Controllers
{
    [Authorize(Roles = "Vendedor")]
    public class VendedorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AlmacenamientoImagenProducto _almacenamientoImagen;

        public VendedorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, AlmacenamientoImagenProducto almacenamientoImagen)
        {
            _db = db;
            _userManager = userManager;
            _almacenamientoImagen = almacenamientoImagen;
        }

        private async Task<IActionResult?> VerificarVendedorActivoAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _db.Vendedores.AsNoTracking()
                .FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);

            return vendedor != null && !vendedor.Activo ? Forbid() : null;
        }

        private async Task<(Vendedor? Vendedor, IActionResult? Resultado)> ObtenerVendedorActualAsync()
        {
            var restriccion = await VerificarVendedorActivoAsync();
            if (restriccion != null) return (null, restriccion);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return (null, Challenge());

            var vendedor = await _db.Vendedores
                .FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);

            return vendedor == null ? (null, NotFound()) : (vendedor, null);
        }

        // GET: Vendedor/MisProductos
        public async Task<IActionResult> MisProductos()
        {
            var restriccion = await VerificarVendedorActivoAsync();
            if (restriccion != null) return restriccion;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _db.Vendedores.Include(v => v.Productos)
                .FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);

            if (vendedor == null) return RedirectToAction("CrearPerfil");

            var productos = vendedor.Productos?.OrderBy(p => p.Nombre).ToList() ?? new List<Producto>();

            return View(productos);
        }

        // GET: Vendedor/Crear
        public async Task<IActionResult> Crear()
        {
            var restriccion = await VerificarVendedorActivoAsync();
            if (restriccion != null) return restriccion;

            ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            return View();
        }

        // POST: Vendedor/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto model, IFormFile? imagenArchivo = null, string modoImagen = "url")
        {
            ViewBag.ModoImagen = modoImagen;
            var restriccion = await VerificarVendedorActivoAsync();
            if (restriccion != null) return restriccion;

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.ApplicationUserId == user.Id);
            if (vendedor == null)
            {
                // Si no tiene perfil de vendedor, crear uno básico con nombre tienda por defecto
                vendedor = new Vendedor
                {
                    ApplicationUserId = user.Id,
                    NombreTienda = $"{user.Nombre} {user.Apellido} - Tienda",
                    Activo = true
                };
                _db.Vendedores.Add(vendedor);
                await _db.SaveChangesAsync();
            }

            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, model.ImagenUrl, imagenArchivo, null, esNuevo: true);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(model.ImagenUrl), resultadoImagen.Error!);
            }

            if (!ModelState.IsValid)
            {
                _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                return View(model);
            }

            // Asignar vendedor y guardar producto
            model.VendedorId = vendedor.Id;
            model.Activo = true;
            model.ImagenUrl = resultadoImagen.Url;
            _db.Productos.Add(model);
            try
            {
                await _db.SaveChangesAsync();
            }
            catch
            {
                _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                throw;
            }

            return RedirectToAction(nameof(MisProductos));
        }

        // GET: Vendedor/EditarProducto/5
        public async Task<IActionResult> EditarProducto(int id)
        {
            var (vendedor, resultado) = await ObtenerVendedorActualAsync();
            if (resultado != null) return resultado;

            var producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.VendedorId == vendedor!.Id);
            if (producto == null) return NotFound();

            ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            return View(producto);
        }

        // POST: Vendedor/EditarProducto/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarProducto(int id, Producto model, IFormFile? imagenArchivo = null, string modoImagen = "url")
        {
            ViewBag.ModoImagen = modoImagen;
            var (vendedor, resultado) = await ObtenerVendedorActualAsync();
            if (resultado != null) return resultado;
            if (id != model.Id) return NotFound();

            var producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.VendedorId == vendedor!.Id);
            if (producto == null) return NotFound();

            var imagenAnterior = producto.ImagenUrl;
            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, model.ImagenUrl, imagenArchivo, imagenAnterior, esNuevo: false);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(model.ImagenUrl), resultadoImagen.Error!);
                model.ImagenUrl = imagenAnterior;
            }

            if (!await _db.Categorias.AnyAsync(c => c.Id == model.CategoriaId))
            {
                ModelState.AddModelError(nameof(model.CategoriaId), "Selecciona una categoría válida.");
            }

            if (!ModelState.IsValid)
            {
                if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                {
                    _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                }
                ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                ViewBag.ImagenActual = imagenAnterior;
                return View(model);
            }

            producto.Nombre = model.Nombre;
            producto.Marca = model.Marca;
            producto.Modelo = model.Modelo;
            producto.Especificaciones = model.Especificaciones;
            producto.SKU = model.SKU;
            producto.Precio = model.Precio;
            producto.Stock = model.Stock;
            producto.GarantiaMeses = model.GarantiaMeses;
            producto.ImagenUrl = resultadoImagen.Url;
            producto.Activo = model.Activo;
            producto.CategoriaId = model.CategoriaId;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                {
                    _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                }
                ModelState.AddModelError(nameof(model.SKU), "No se pudo guardar el producto. Comprueba que el SKU no esté en uso.");
                model.ImagenUrl = imagenAnterior;
                ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                ViewBag.ModoImagen = modoImagen;
                return View(model);
            }

            if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
            {
                _almacenamientoImagen.EliminarImagenSubida(imagenAnterior);
            }

            TempData["Mensaje"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(MisProductos));
        }

        // GET: Vendedor/EliminarProducto/5
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var (vendedor, resultado) = await ObtenerVendedorActualAsync();
            if (resultado != null) return resultado;

            var producto = await _db.Productos
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.VendedorId == vendedor!.Id);
            if (producto == null) return NotFound();

            ViewBag.TienePedidos = await _db.DetallesOrden.AnyAsync(d => d.ProductoId == id);
            return View(producto);
        }

        // POST: Vendedor/EliminarProducto/5
        [HttpPost, ActionName("EliminarProducto")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarProductoConfirmado(int id)
        {
            var (vendedor, resultado) = await ObtenerVendedorActualAsync();
            if (resultado != null) return resultado;

            var producto = await _db.Productos
                .FirstOrDefaultAsync(p => p.Id == id && p.VendedorId == vendedor!.Id);
            if (producto == null) return NotFound();

            if (await _db.DetallesOrden.AnyAsync(d => d.ProductoId == id))
            {
                TempData["Error"] = "No se puede eliminar un producto asociado a pedidos. Puedes desactivarlo desde la edición.";
                return RedirectToAction(nameof(MisProductos));
            }

            _db.Productos.Remove(producto);
            await _db.SaveChangesAsync();

            TempData["Mensaje"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(MisProductos));
        }
    }
}