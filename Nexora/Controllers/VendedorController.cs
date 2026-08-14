using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;

namespace Nexora.Controllers
{
    [Authorize(Roles = "Vendedor")]
    public class VendedorController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public VendedorController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // GET: Vendedor/MisProductos
        public async Task<IActionResult> MisProductos()
        {
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
            ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            return View();
        }

        // POST: Vendedor/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto model)
        {
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

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _db.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                return View(model);
            }

            // Asignar vendedor y guardar producto
            model.VendedorId = vendedor.Id;
            model.Activo = true;
            _db.Productos.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(MisProductos));
        }
    }
}