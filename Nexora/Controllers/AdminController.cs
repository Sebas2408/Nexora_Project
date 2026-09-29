using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.ViewModels;

namespace Nexora.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AdminController> _logger;
        private const int TamanoPagina = 10;

        public AdminController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AdminController> logger)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        // GET: Admin/
        // Redirige al listado de vendedores para que /Admin/ tenga una entrada válida
        public IActionResult Index ( )
        {
            return RedirectToAction ( nameof ( Vendedores ) );
        }

        // GET: Admin/Vendedores
        // Lista todos los vendedores registrados con opción de búsqueda
        public async Task<IActionResult> Vendedores(string? q, bool? activo, string orden = "tienda", bool ascendente = true, int pagina = 1)
        {
            var query = _db.Vendedores.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(v =>
                    v.NombreTienda.Contains(q) ||
                    (v.ApplicationUser != null && (v.ApplicationUser.Nombre.Contains(q) ||
                    v.ApplicationUser.Apellido.Contains(q) ||
                    (v.ApplicationUser.Email != null && v.ApplicationUser.Email.Contains(q)))));
            }

            if (activo.HasValue)
            {
                query = query.Where(v => v.Activo == activo.Value);
            }

            query = (orden.ToLowerInvariant(), ascendente) switch
            {
                ("fecha", true) => query.OrderBy(v => v.FechaRegistro).ThenBy(v => v.Id),
                ("fecha", false) => query.OrderByDescending(v => v.FechaRegistro).ThenBy(v => v.Id),
                ("productos", true) => query.OrderBy(v => _db.Productos.Count(p => p.VendedorId == v.Id)).ThenBy(v => v.Id),
                ("productos", false) => query.OrderByDescending(v => _db.Productos.Count(p => p.VendedorId == v.Id)).ThenBy(v => v.Id),
                ("estado", true) => query.OrderBy(v => v.Activo).ThenBy(v => v.NombreTienda),
                ("estado", false) => query.OrderByDescending(v => v.Activo).ThenBy(v => v.NombreTienda),
                (_, false) => query.OrderByDescending(v => v.NombreTienda).ThenBy(v => v.Id),
                _ => query.OrderBy(v => v.NombreTienda).ThenBy(v => v.Id)
            };

            var totalResultados = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalResultados / (double)TamanoPagina);
            pagina = Math.Clamp(pagina, 1, Math.Max(totalPaginas, 1));

            var vendedores = await query
                .Skip((pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .Select(v => new VendedorAdminViewModel
                {
                    Id = v.Id,
                    NombreTienda = v.NombreTienda,
                    NombreCompleto = v.ApplicationUser == null
                        ? "Sin usuario asociado"
                        : v.ApplicationUser.Nombre + " " + v.ApplicationUser.Apellido,
                    Email = v.ApplicationUser == null ? "" : v.ApplicationUser.Email ?? "",
                    FechaRegistro = v.FechaRegistro,
                    Activo = v.Activo,
                    CantidadProductos = _db.Productos.Count(p => p.VendedorId == v.Id),
                    IngresosHistoricos = _db.DetallesOrden
                        .Where(d => d.Producto != null && d.Producto.VendedorId == v.Id &&
                            d.Orden != null && d.Orden.Estado != EstadoEnvio.Cancelado)
                        .Sum(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0m
                })
                .ToListAsync();

            foreach (var vendedor in vendedores)
            {
                var separador = vendedor.Email.LastIndexOf('@');
                vendedor.Dominio = separador >= 0 ? vendedor.Email[(separador + 1)..] : "";
            }

            var vm = new AdminVendedoresViewModel
            {
                Vendedores = vendedores,
                Query = q,
                Activo = activo,
                Orden = orden,
                Ascendente = ascendente,
                Pagina = pagina,
                TamanoPagina = TamanoPagina,
                TotalResultados = totalResultados,
                TotalVendedores = await _db.Vendedores.CountAsync(),
                TotalActivos = await _db.Vendedores.CountAsync(v => v.Activo),
                TotalSuspendidos = await _db.Vendedores.CountAsync(v => !v.Activo)
            };

            return View(vm);
        }

        // GET: Admin/DetalleVendedor/5
        // Muestra la información completa de un vendedor y su catálogo
        public async Task<IActionResult> DetalleVendedor ( int id )
        {
            var vendedor = await _db.Vendedores
                .AsNoTracking()
                .Include(v => v.ApplicationUser)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendedor == null) return NotFound();

            var productosQuery = _db.Productos.Where(p => p.VendedorId == id);
            var productos = await productosQuery
                .AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            var detallesVendedor = _db.DetallesOrden
                .Where(d => d.Producto != null && d.Producto.VendedorId == id);
            var pedidosAsociados = await detallesVendedor.Select(d => d.OrdenId).Distinct().CountAsync();
            var ventasValidas = detallesVendedor.Where(d => d.Orden != null && d.Orden.Estado != EstadoEnvio.Cancelado);
            var unidadesVendidas = await ventasValidas.SumAsync(d => (int?)d.Cantidad) ?? 0;
            var ingresosHistoricos = await ventasValidas.SumAsync(d => (decimal?)(d.Cantidad * d.PrecioUnitario)) ?? 0m;
            var actividadReciente = await detallesVendedor
                .Where(d => d.Orden != null)
                .MaxAsync(d => (DateTime?)d.Orden!.FechaCreacion);

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
                FechaRegistro = vendedor.FechaRegistro,
                ProductosActivos = productos.Count(p => p.Activo),
                ProductosInactivos = productos.Count(p => !p.Activo),
                PedidosAsociados = pedidosAsociados,
                UnidadesVendidas = unidadesVendidas,
                IngresosHistoricos = ingresosHistoricos,
                ActividadReciente = actividadReciente,
                Productos = productos
            };

            return View ( vm );
        }

        public async Task<IActionResult> Inventario(
            string? q,
            int? vendedorId,
            int? categoriaId,
            string estado = "todos",
            string existencias = "todas",
            int umbralStockBajo = 5,
            int pagina = 1)
        {
            umbralStockBajo = Math.Clamp(umbralStockBajo, 1, 10000);
            estado = estado.ToLowerInvariant() is "activo" or "inactivo" ? estado.ToLowerInvariant() : "todos";
            existencias = existencias.ToLowerInvariant() is "bajo" or "cero" or "normal" ? existencias.ToLowerInvariant() : "todas";

            var query = _db.Productos.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(q))
            {
                q = q.Trim();
                query = query.Where(p => p.Nombre.Contains(q) || p.Marca.Contains(q) ||
                    (p.Modelo != null && p.Modelo.Contains(q)) || p.SKU.Contains(q));
            }
            if (vendedorId.HasValue)
            {
                query = query.Where(p => p.VendedorId == vendedorId.Value);
            }
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }
            if (estado == "activo") query = query.Where(p => p.Activo);
            if (estado == "inactivo") query = query.Where(p => !p.Activo);
            if (existencias == "bajo") query = query.Where(p => p.Stock > 0 && p.Stock <= umbralStockBajo);
            if (existencias == "cero") query = query.Where(p => p.Stock == 0);
            if (existencias == "normal") query = query.Where(p => p.Stock > umbralStockBajo);

            var totalResultados = await query.CountAsync();
            var totalPaginas = (int)Math.Ceiling(totalResultados / (double)TamanoPagina);
            pagina = Math.Clamp(pagina, 1, Math.Max(totalPaginas, 1));

            var productos = await query
                .OrderBy(p => p.Nombre)
                .ThenBy(p => p.Id)
                .Skip((pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .Select(p => new AdminProductoViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    SKU = p.SKU,
                    Categoria = p.Categoria == null ? "—" : p.Categoria.Nombre,
                    CategoriaId = p.CategoriaId,
                    Vendedor = p.Vendedor == null ? "—" : p.Vendedor.NombreTienda,
                    VendedorId = p.VendedorId,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    Precio = p.Precio
                })
                .ToListAsync();

            var vm = new AdminInventarioViewModel
            {
                Productos = productos,
                Categorias = await _db.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync(),
                Vendedores = await _db.Vendedores.AsNoTracking()
                    .OrderBy(v => v.NombreTienda)
                    .Select(v => new AdminVendedorOpcionViewModel { Id = v.Id, Nombre = v.NombreTienda })
                    .ToListAsync(),
                Query = q,
                VendedorId = vendedorId,
                CategoriaId = categoriaId,
                Estado = estado,
                Existencias = existencias,
                UmbralStockBajo = umbralStockBajo,
                Pagina = pagina,
                TamanoPagina = TamanoPagina,
                TotalResultados = totalResultados
            };

            return View(vm);
        }

        public async Task<IActionResult> DetalleProducto(int id)
        {
            var producto = await _db.Productos
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new AdminProductoDetalleViewModel
                {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    SKU = p.SKU,
                    Categoria = p.Categoria == null ? "—" : p.Categoria.Nombre,
                    CategoriaId = p.CategoriaId,
                    Vendedor = p.Vendedor == null ? "—" : p.Vendedor.NombreTienda,
                    VendedorId = p.VendedorId,
                    Stock = p.Stock,
                    Activo = p.Activo,
                    Precio = p.Precio,
                    Marca = p.Marca,
                    Modelo = p.Modelo,
                    Especificaciones = p.Especificaciones,
                    ImagenUrl = p.ImagenUrl,
                    GarantiaMeses = p.GarantiaMeses
                })
                .FirstOrDefaultAsync();

            return producto == null ? NotFound() : View(producto);
        }

        public async Task<IActionResult> Analiticas(DateTime? desde, DateTime? hasta)
        {
            const int umbralStockBajo = 5;
            var pedidos = _db.Ordenes.AsNoTracking().AsQueryable();
            if (desde.HasValue)
            {
                pedidos = pedidos.Where(o => o.FechaCreacion >= desde.Value.Date);
            }
            if (hasta.HasValue)
            {
                var limiteSuperior = hasta.Value.Date.AddDays(1);
                pedidos = pedidos.Where(o => o.FechaCreacion < limiteSuperior);
            }

            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            {
                ModelState.AddModelError(string.Empty, "La fecha inicial no puede ser posterior a la fecha final.");
            }

            var conteosPorEstado = await pedidos
                .GroupBy(o => o.Estado)
                .Select(grupo => new { Estado = grupo.Key, Cantidad = grupo.Count() })
                .ToListAsync();
            var conteosEstado = conteosPorEstado.ToDictionary(x => x.Estado, x => x.Cantidad);

            var detalles = _db.DetallesOrden.AsNoTracking()
                .Where(d => d.Orden != null && d.Producto != null && d.Orden.Estado != EstadoEnvio.Cancelado);
            if (desde.HasValue)
            {
                detalles = detalles.Where(d => d.Orden!.FechaCreacion >= desde.Value.Date);
            }
            if (hasta.HasValue)
            {
                var limiteSuperior = hasta.Value.Date.AddDays(1);
                detalles = detalles.Where(d => d.Orden!.FechaCreacion < limiteSuperior);
            }

            var vm = new AdminAnaliticasViewModel
            {
                Desde = desde,
                Hasta = hasta,
                TotalVendedores = await _db.Vendedores.CountAsync(),
                VendedoresActivos = await _db.Vendedores.CountAsync(v => v.Activo),
                VendedoresSuspendidos = await _db.Vendedores.CountAsync(v => !v.Activo),
                TotalProductos = await _db.Productos.CountAsync(),
                ProductosActivos = await _db.Productos.CountAsync(p => p.Activo),
                ProductosInactivos = await _db.Productos.CountAsync(p => !p.Activo),
                ExistenciasBajas = await _db.Productos.CountAsync(p => p.Stock > 0 && p.Stock <= umbralStockBajo),
                SinExistencias = await _db.Productos.CountAsync(p => p.Stock == 0),
                UmbralStockBajo = umbralStockBajo,
                TotalPedidos = await pedidos.CountAsync(),
                Ingresos = await pedidos
                    .Where(o => o.Estado != EstadoEnvio.Cancelado)
                    .SumAsync(o => (decimal?)o.Total) ?? 0m,
                PedidosPorEstado = Enum.GetValues<EstadoEnvio>()
                    .Select(estado => new AdminEstadoPedidoViewModel
                    {
                        Estado = estado.ToString(),
                        Cantidad = conteosEstado.GetValueOrDefault(estado)
                    })
                    .ToList()
            };

            vm.MejoresVendedores = await detalles
                .GroupBy(d => new { d.Producto!.VendedorId, Nombre = d.Producto.Vendedor!.NombreTienda })
                .Select(grupo => new AdminTopVendedorViewModel
                {
                    Nombre = grupo.Key.Nombre,
                    Ingresos = grupo.Sum(d => d.Cantidad * d.PrecioUnitario),
                    UnidadesVendidas = grupo.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.Ingresos)
                .Take(5)
                .ToListAsync();

            vm.MejoresProductos = await detalles
                .GroupBy(d => new { d.ProductoId, Nombre = d.Producto!.Nombre, Vendedor = d.Producto.Vendedor!.NombreTienda })
                .Select(grupo => new AdminTopProductoViewModel
                {
                    Nombre = grupo.Key.Nombre,
                    Vendedor = grupo.Key.Vendedor,
                    Ingresos = grupo.Sum(d => d.Cantidad * d.PrecioUnitario),
                    UnidadesVendidas = grupo.Sum(d => d.Cantidad)
                })
                .OrderByDescending(x => x.UnidadesVendidas)
                .Take(5)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> PerfilAdministrador()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            return View(new AdminPerfilViewModel
            {
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CerrarSesion()
        {
            await _signInManager.SignOutAsync();
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        [HttpGet]
        public IActionResult RegistrarVendedor()
        {
            return View(new RegistroVendedorViewModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditarVendedor(int id)
        {
            var vendedor = await _db.Vendedores
                .AsNoTracking()
                .Include(v => v.ApplicationUser)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendedor?.ApplicationUser == null) return NotFound();

            ViewBag.Correo = vendedor.ApplicationUser.Email ?? string.Empty;
            return View(new EditarVendedorViewModel
            {
                Id = vendedor.Id,
                NombreTienda = vendedor.NombreTienda,
                Descripcion = vendedor.Descripcion,
                Logo = vendedor.Logo,
                Nombre = vendedor.ApplicationUser.Nombre,
                Apellido = vendedor.ApplicationUser.Apellido,
                Direccion = vendedor.ApplicationUser.Direccion
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarVendedor(int id, EditarVendedorViewModel model)
        {
            if (id != model.Id) return NotFound();

            var vendedor = await _db.Vendedores
                .Include(v => v.ApplicationUser)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vendedor?.ApplicationUser == null) return NotFound();

            ViewBag.Correo = vendedor.ApplicationUser.Email ?? string.Empty;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            vendedor.NombreTienda = model.NombreTienda.Trim();
            vendedor.Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();
            vendedor.Logo = string.IsNullOrWhiteSpace(model.Logo) ? null : model.Logo.Trim();
            vendedor.ApplicationUser.Nombre = model.Nombre.Trim();
            vendedor.ApplicationUser.Apellido = model.Apellido.Trim();
            vendedor.ApplicationUser.Direccion = string.IsNullOrWhiteSpace(model.Direccion) ? null : model.Direccion.Trim();

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                _logger.LogError(exception, "Error al editar el perfil del vendedor {VendedorId} desde el panel administrativo.", id);
                ModelState.AddModelError(string.Empty, "No se pudieron guardar los cambios. Inténtalo de nuevo.");
                return View(model);
            }

            TempData["Mensaje"] = $"Los datos de {vendedor.NombreTienda} se actualizaron correctamente.";
            return RedirectToAction(nameof(DetalleVendedor), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVendedor(RegistroVendedorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();
            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo electrónico.");
                return View(model);
            }

            await using var transaction = await _db.Database.BeginTransactionAsync();
            try
            {
                if (!await _roleManager.RoleExistsAsync("Vendedor"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole("Vendedor"));
                    if (!roleResult.Succeeded)
                    {
                        await transaction.RollbackAsync();
                        ModelState.AddModelError(string.Empty, "No se pudo preparar el rol de vendedor.");
                        return View(model);
                    }
                }

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    Nombre = model.Nombre.Trim(),
                    Apellido = model.Apellido.Trim(),
                    Direccion = string.IsNullOrWhiteSpace(model.Direccion) ? null : model.Direccion.Trim(),
                    LockoutEnabled = true
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                var roleAssignment = await _userManager.AddToRoleAsync(user, "Vendedor");
                if (!roleAssignment.Succeeded)
                {
                    await transaction.RollbackAsync();
                    foreach (var error in roleAssignment.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }

                _db.Vendedores.Add(new Vendedor
                {
                    ApplicationUserId = user.Id,
                    NombreTienda = model.NombreTienda.Trim(),
                    Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim(),
                    Logo = string.IsNullOrWhiteSpace(model.Logo) ? null : model.Logo.Trim(),
                    Activo = true,
                    FechaRegistro = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Mensaje"] = $"El comerciante {model.NombreTienda.Trim()} se registró correctamente.";
                return RedirectToAction(nameof(Vendedores));
            }
            catch (DbUpdateException exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception, "Error de persistencia al registrar un vendedor desde el panel administrativo.");
                ModelState.AddModelError(string.Empty, "No se pudo completar el registro. Comprueba que el correo no esté ya registrado e inténtalo de nuevo.");
                return View(model);
            }
        }

        // POST: Admin/ToggleActivo/5
        // Activa o desactiva a un vendedor (bloquea/permite su visibilidad en el catálogo)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstadoVendedor(int id, bool activo, string? returnUrl)
        {
            var vendedor = await _db.Vendedores.FirstOrDefaultAsync(v => v.Id == id);
            if (vendedor == null) return NotFound();

            vendedor.Activo = activo;
            await _db.SaveChangesAsync();

            TempData["Mensaje"] = vendedor.Activo
                ? $"Vendedor \"{vendedor.NombreTienda}\" reactivado correctamente."
                : $"Vendedor \"{vendedor.NombreTienda}\" suspendido correctamente.";

            // Se valida returnUrl para evitar redirecciones abiertas (open redirect)
            if ( !string.IsNullOrEmpty ( returnUrl ) && Url.IsLocalUrl ( returnUrl ) )
            {
                return LocalRedirect ( returnUrl );
            }

            return RedirectToAction(nameof(Vendedores));
        }
    }
}