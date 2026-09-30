using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.Services;

namespace Nexora.Controllers
{
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AlmacenamientoImagenProducto _almacenamientoImagen;

        public InventarioController(ApplicationDbContext context, AlmacenamientoImagenProducto almacenamientoImagen)
        {
            _context = context;
            _almacenamientoImagen = almacenamientoImagen;
        }

        // GET: Inventario
        public async Task<IActionResult> Index(string? q, int? categoriaId, string? estado)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .AsQueryable();

            // Búsqueda por SKU, nombre o modelo
            if (!string.IsNullOrWhiteSpace(q))
            {
                var t = q.Trim();
                query = query.Where(p => p.SKU.Contains(t) || p.Nombre.Contains(t) || p.Modelo.Contains(t));
            }

            // Filtrado por categoría
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            // Filtrado por estado activo/inactivo
            if (!string.IsNullOrWhiteSpace(estado))
            {
                if (estado.Equals("activo", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Activo);
                }
                else if (estado.Equals("inactivo", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(p => !p.Activo);
                }
            }

            query = query.OrderBy(p => p.Nombre);

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Query = q ?? string.Empty;
            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.EstadoSeleccionado = estado ?? string.Empty;

            return View(await query.ToListAsync());
        }

        // GET: Inventario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Inventario/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre");
            ViewData["VendedorId"] = new SelectList(_context.Vendedores.OrderBy(v => v.NombreTienda), "Id", "NombreTienda");
            return View();
        }

        // POST: Inventario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Marca,Modelo,Especificaciones,SKU,Precio,Stock,GarantiaMeses,ImagenUrl,Activo,CategoriaId,VendedorId")] Producto producto,
            IFormFile? imagenArchivo = null,
            string modoImagen = "url")
        {
            NormalizarProducto(producto);
            await ValidarProductoAsync(producto);

            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, producto.ImagenUrl, imagenArchivo, null, esNuevo: true);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(producto.ImagenUrl), resultadoImagen.Error!);
            }

            if (ModelState.IsValid && await _context.Productos.AnyAsync(p => p.SKU == producto.SKU))
            {
                ModelState.AddModelError(nameof(producto.SKU), "Ya existe un producto con este SKU.");
            }

            if (ModelState.IsValid)
            {
                producto.ImagenUrl = resultadoImagen.Url;
                _context.Add(producto);
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqlException sqlException && (sqlException.Number == 2601 || sqlException.Number == 2627))
                {
                    _context.Entry(producto).State = EntityState.Detached;
                    ModelState.AddModelError(nameof(producto.SKU), "Ya existe un producto con este SKU.");
                    _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                }
            }
            else
            {
                _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", producto.CategoriaId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores.OrderBy(v => v.NombreTienda), "Id", "NombreTienda", producto.VendedorId);
            ViewBag.ModoImagen = modoImagen;
            return View(producto);
        }

        // GET: Inventario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", producto.CategoriaId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores.OrderBy(v => v.NombreTienda), "Id", "NombreTienda", producto.VendedorId);
            return View(producto);
        }

        // POST: Inventario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nombre,Marca,Modelo,Especificaciones,SKU,Precio,Stock,GarantiaMeses,ImagenUrl,Activo,CategoriaId,VendedorId")] Producto producto,
            IFormFile? imagenArchivo = null,
            string modoImagen = "url")
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            var productoExistente = await _context.Productos.FindAsync(id);
            if (productoExistente == null)
            {
                return NotFound();
            }

            NormalizarProducto(producto);
            await ValidarProductoAsync(producto, producto.Id);

            var imagenAnterior = productoExistente.ImagenUrl;
            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, producto.ImagenUrl, imagenArchivo, imagenAnterior, esNuevo: false);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(producto.ImagenUrl), resultadoImagen.Error!);
            }

            if (ModelState.IsValid && await _context.Productos
                .AnyAsync(p => p.SKU == producto.SKU && p.Id != producto.Id))
            {
                ModelState.AddModelError(nameof(producto.SKU), "Ya existe otro producto con este SKU.");
            }

            if (ModelState.IsValid)
            {
                productoExistente.Nombre = producto.Nombre;
                productoExistente.Marca = producto.Marca;
                productoExistente.Modelo = producto.Modelo;
                productoExistente.Especificaciones = producto.Especificaciones;
                productoExistente.SKU = producto.SKU;
                productoExistente.Precio = producto.Precio;
                productoExistente.Stock = producto.Stock;
                productoExistente.GarantiaMeses = producto.GarantiaMeses;
                productoExistente.ImagenUrl = resultadoImagen.Url;
                productoExistente.Activo = producto.Activo;
                productoExistente.CategoriaId = producto.CategoriaId;
                productoExistente.VendedorId = producto.VendedorId;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                    {
                        _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                    }
                    if (!ProductoExists(producto.Id)) return NotFound();
                    throw;
                }
                catch
                {
                    if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                    {
                        _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                    }
                    throw;
                }

                if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                {
                    _almacenamientoImagen.EliminarImagenSubida(imagenAnterior);
                }

                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (!string.Equals(resultadoImagen.Url, imagenAnterior, StringComparison.Ordinal))
                {
                    _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                }
            }

            ViewData["CategoriaId"] = new SelectList(_context.Categorias.OrderBy(c => c.Nombre), "Id", "Nombre", producto.CategoriaId);
            ViewData["VendedorId"] = new SelectList(_context.Vendedores.OrderBy(v => v.NombreTienda), "Id", "NombreTienda", producto.VendedorId);
            ViewBag.ModoImagen = modoImagen;
            ViewBag.ImagenActual = imagenAnterior;
            producto.ImagenUrl = imagenAnterior;
            return View(producto);
        }

        // GET: Inventario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Inventario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        private static void NormalizarProducto(Producto producto)
        {
            producto.Nombre = producto.Nombre?.Trim() ?? string.Empty;
            producto.Marca = producto.Marca?.Trim() ?? string.Empty;
            producto.Modelo = string.IsNullOrWhiteSpace(producto.Modelo) ? null : producto.Modelo.Trim();
            producto.Especificaciones = producto.Especificaciones?.Trim() ?? string.Empty;
            producto.SKU = producto.SKU?.Trim() ?? string.Empty;
        }

        private async Task ValidarProductoAsync(Producto producto, int? idExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
                ModelState.AddModelError(nameof(producto.Nombre), "El nombre del producto es obligatorio.");

            if (string.IsNullOrWhiteSpace(producto.Marca))
                ModelState.AddModelError(nameof(producto.Marca), "La marca es obligatoria.");

            if (string.IsNullOrWhiteSpace(producto.Especificaciones))
                ModelState.AddModelError(nameof(producto.Especificaciones), "Las especificaciones son obligatorias.");

            if (string.IsNullOrWhiteSpace(producto.SKU))
                ModelState.AddModelError(nameof(producto.SKU), "El SKU es obligatorio.");
            else if (await _context.Productos.AnyAsync(p => p.SKU == producto.SKU && p.Id != idExcluir))
                ModelState.AddModelError(nameof(producto.SKU), "Ya existe un producto con este SKU.");

            if (producto.CategoriaId <= 0 || !await _context.Categorias.AnyAsync(c => c.Id == producto.CategoriaId))
                ModelState.AddModelError(nameof(producto.CategoriaId), "Selecciona una categoría válida.");

            if (producto.VendedorId <= 0 || !await _context.Vendedores.AnyAsync(v => v.Id == producto.VendedorId))
                ModelState.AddModelError(nameof(producto.VendedorId), "Selecciona un vendedor válido.");
        }
    }
}
