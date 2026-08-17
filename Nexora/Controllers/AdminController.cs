using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.ViewModels;

namespace Nexora.Controllers
{
    // Solo usuarios con rol Administrador pueden acceder a este controlador
    [Authorize ( Roles = "Administrador" )]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AdminController ( ApplicationDbContext db )
        {
            _db = db;
        }

        // GET: Admin/
        // Redirige al listado de vendedores para que /Admin/ tenga una entrada válida
        public IActionResult Index ( )
        {
            return RedirectToAction ( nameof ( Vendedores ) );
        }

        // GET: Admin/Vendedores
        // Lista todos los vendedores registrados con opción de búsqueda
        public async Task<IActionResult> Vendedores ( string? q )
        {
            var query = _db.Vendedores
                .Include(v => v.ApplicationUser)
                .Include(v => v.Productos)
                .AsQueryable();

            if ( !string.IsNullOrWhiteSpace ( q ) )
            {
                q = q.Trim ();
                query = query.Where ( v =>
                    v.NombreTienda.Contains ( q ) ||
                    ( v.ApplicationUser != null && v.ApplicationUser.Nombre.Contains ( q ) ) ||
                    ( v.ApplicationUser != null && v.ApplicationUser.Apellido.Contains ( q ) ) ||
                    ( v.ApplicationUser != null && v.ApplicationUser.Email != null && v.ApplicationUser.Email.Contains ( q ) ) );
            }

            var vendedores = await query.OrderBy(v => v.NombreTienda).ToListAsync();

            // Se mapea a ViewModel para no exponer datos sensibles del usuario (hash, stamps, etc.)
            var vm = vendedores.Select(v => new VendedorAdminViewModel
            {
                Id = v.Id,
                NombreTienda = v.NombreTienda,
                NombreCompleto = v.ApplicationUser != null
                    ? $"{v.ApplicationUser.Nombre} {v.ApplicationUser.Apellido}"
                    : "Sin usuario asociado",
                Email = v.ApplicationUser?.Email ?? "—",
                Activo = v.Activo,
                CantidadProductos = v.Productos?.Count ?? 0
            }).ToList();

            ViewBag.Query = q;
            ViewBag.TotalVendedores = vm.Count;
            ViewBag.TotalActivos = vm.Count ( x => x.Activo );
            ViewBag.TotalInactivos = vm.Count ( x => !x.Activo );

            return View ( vm );
        }

        // GET: Admin/DetalleVendedor/5
        // Muestra la información completa de un vendedor y su catálogo
        public async Task<IActionResult> DetalleVendedor ( int id )
        {
            var vendedor = await _db.Vendedores
                .Include(v => v.ApplicationUser)
                .Include(v => v.Productos!).ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);

            if ( vendedor == null ) return NotFound ();

            var vm = new VendedorDetalleViewModel
            {
                Id = vendedor.Id,
                NombreTienda = vendedor.NombreTienda,
                Descripcion = vendedor.Descripcion,
                Logo = vendedor.Logo,
                Activo = vendedor.Activo,
                NombreCompleto = vendedor.ApplicationUser != null
                    ? $"{vendedor.ApplicationUser.Nombre} {vendedor.ApplicationUser.Apellido}"
                    : "Sin usuario asociado",
                Email = vendedor.ApplicationUser?.Email ?? "—",
                Direccion = vendedor.ApplicationUser?.Direccion,
                Productos = vendedor.Productos?.OrderBy(p => p.Nombre).ToList() ?? new List<Producto>()
            };

            return View ( vm );
        }

        // POST: Admin/ToggleActivo/5
        // Activa o desactiva a un vendedor (bloquea/permite su visibilidad en el catálogo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActivo ( int id, string? returnUrl )
        {
            var vendedor = await _db.Vendedores.FindAsync(id);
            if ( vendedor == null ) return NotFound ();

            vendedor.Activo = !vendedor.Activo;
            await _db.SaveChangesAsync ();

            TempData["Mensaje"] = vendedor.Activo
                ? $"Vendedor \"{vendedor.NombreTienda}\" activado correctamente."
                : $"Vendedor \"{vendedor.NombreTienda}\" desactivado correctamente.";

            // Se valida returnUrl para evitar redirecciones abiertas (open redirect)
            if ( !string.IsNullOrEmpty ( returnUrl ) && Url.IsLocalUrl ( returnUrl ) )
            {
                return LocalRedirect ( returnUrl );
            }

            return RedirectToAction ( nameof ( Vendedores ) );
        }
    }
}