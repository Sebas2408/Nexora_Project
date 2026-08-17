using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.ViewModels;

namespace Nexora.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController ( ApplicationDbContext db )
        {
            _db = db;
        }

        // Cantidad de productos que se muestran por página en el catálogo
        private const int TamanoPagina = 9;

        // GET: Home/Index
        public async Task<IActionResult> Index ( int? categoriaId, string? q, decimal? precioMin, decimal? precioMax, string? orden, int pagina = 1 )
        {
            if ( pagina < 1 ) pagina = 1;

            var query = _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .Where(p => p.Activo);

            if ( categoriaId.HasValue )
            {
                query = query.Where ( p => p.CategoriaId == categoriaId.Value );
            }

            if ( !string.IsNullOrWhiteSpace ( q ) )
            {
                q = q.Trim ();
                query = query.Where ( p => p.Nombre.Contains ( q ) || p.Marca.Contains ( q ) );
            }

            if ( precioMin.HasValue )
            {
                query = query.Where ( p => p.Precio >= precioMin.Value );
            }

            if ( precioMax.HasValue )
            {
                query = query.Where ( p => p.Precio <= precioMax.Value );
            }

            // Orden dinámico elegido por el usuario (por defecto: nombre A-Z)
            query = orden switch
            {
                "precio_asc" => query.OrderBy ( p => p.Precio ),
                "precio_desc" => query.OrderByDescending ( p => p.Precio ),
                "nombre_desc" => query.OrderByDescending ( p => p.Nombre ),
                _ => query.OrderBy ( p => p.Nombre )
            };

            var totalProductos = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalProductos / (double)TamanoPagina);
            if ( totalPaginas > 0 && pagina > totalPaginas ) pagina = totalPaginas;

            var productos = await query
                .Skip((pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToListAsync();

            var vm = productos.Select(p => new ProductoViewModel
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Marca = p.Marca,
                Modelo = p.Modelo,
                Especificaciones = p.Especificaciones,
                Precio = p.Precio,
                Stock = p.Stock,
                ImagenUrl = p.ImagenUrl,
                CategoriaNombre = p.Categoria?.Nombre ?? "",
                NombreTienda = p.Vendedor?.NombreTienda ?? ""
            }).ToList();

            // Lista de categorías para filtros
            ViewBag.Categorias = await _db.Categorias.OrderBy ( c => c.Nombre ).ToListAsync ();
            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.Query = q;
            ViewBag.PrecioMin = precioMin;
            ViewBag.PrecioMax = precioMax;
            ViewBag.Orden = orden;
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;
            ViewBag.TotalProductos = totalProductos;

            return View ( vm );
        }

        // GET: Home/Detalle/5
        public async Task<IActionResult> Detalle ( int id )
        {
            var producto = await _db.Productos
                .Include(p => p.Categoria)
                .Include(p => p.Vendedor)
                .FirstOrDefaultAsync(p => p.Id == id && p.Activo);

            if ( producto == null ) return NotFound ();

            var vm = new ProductoViewModel
            {
                Id = producto.Id,
                Nombre = producto.Nombre,
                Marca = producto.Marca,
                Modelo = producto.Modelo,
                Especificaciones = producto.Especificaciones,
                Precio = producto.Precio,
                Stock = producto.Stock,
                ImagenUrl = producto.ImagenUrl,
                CategoriaNombre = producto.Categoria?.Nombre ?? "",
                NombreTienda = producto.Vendedor?.NombreTienda ?? ""
            };

            return View ( vm );
        }
    }
}
