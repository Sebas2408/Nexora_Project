using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.ViewModels;

namespace Nexora.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private const string SessionKeyCart = "Cart";

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: Cart/Index
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new List<CarritoItemViewModel>();
            ViewBag.Total = cart.Sum(i => i.Subtotal);
            return View(cart);
        }

        // POST: Cart/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Agregar(int productoId, int cantidad = 1)
        {
            var producto = await _db.Productos.FindAsync(productoId);
            if (producto == null || !producto.Activo) return NotFound();

            var cart = HttpContext.Session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new List<CarritoItemViewModel>();

            var existing = cart.FirstOrDefault(x => x.ProductoId == productoId);
            if (existing != null)
            {
                existing.Cantidad += cantidad;
                if (existing.Cantidad < 1) existing.Cantidad = 1;
            }
            else
            {
                cart.Add(new CarritoItemViewModel
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    ImagenUrl = producto.ImagenUrl,
                    Precio = producto.Precio,
                    Cantidad = Math.Max(1, cantidad)
                });
            }

            HttpContext.Session.SetObject(SessionKeyCart, cart);
            return RedirectToAction("Index");
        }

        // POST: Cart/Quitar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Quitar(int productoId)
        {
            var cart = HttpContext.Session.GetObject<List<CarritoItemViewModel>>(SessionKeyCart) ?? new List<CarritoItemViewModel>();
            var item = cart.FirstOrDefault(x => x.ProductoId == productoId);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.SetObject(SessionKeyCart, cart);
            }

            return RedirectToAction("Index");
        }
    }
}
