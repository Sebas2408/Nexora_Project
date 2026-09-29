using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Productos.Include(p => p.Categoria).Include(p => p.Vendedor);
            return View(await applicationDbContext.ToListAsync());
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
            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, producto.ImagenUrl, imagenArchivo, null, esNuevo: true);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(producto.ImagenUrl), resultadoImagen.Error!);
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
                catch
                {
                    _almacenamientoImagen.EliminarImagenSubida(resultadoImagen.Url);
                    throw;
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

            var imagenAnterior = productoExistente.ImagenUrl;
            var resultadoImagen = await _almacenamientoImagen.ResolverAsync(modoImagen, producto.ImagenUrl, imagenArchivo, imagenAnterior, esNuevo: false);
            if (!resultadoImagen.Correcto)
            {
                ModelState.AddModelError(nameof(producto.ImagenUrl), resultadoImagen.Error!);
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
    }
}
