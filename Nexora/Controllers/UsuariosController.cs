using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nexora.Data;
using Nexora.Models;
using Nexora.ViewModels;

namespace Nexora.Controllers;

[Authorize(Roles = "Administrador")]
[Route("Admin/Usuarios")]
public sealed class UsuariosController : Controller
{
    private const int TamanoPagina = 15;
    private static readonly string[] RolesPermitidos = ["Administrador", "Vendedor", "Cliente"];
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsuariosController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(string? q, int pagina = 1)
    {
        var consulta = _db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            q = q.Trim();
            consulta = consulta.Where(usuario => usuario.Email!.Contains(q) || usuario.Nombre.Contains(q) || usuario.Apellido.Contains(q));
        }
        var total = await consulta.CountAsync();
        var totalPaginas = Math.Max(1, (int)Math.Ceiling(total / (double)TamanoPagina));
        pagina = Math.Clamp(pagina, 1, totalPaginas);
        var usuarios = await consulta.OrderBy(usuario => usuario.Email).Skip((pagina - 1) * TamanoPagina).Take(TamanoPagina).ToListAsync();
        var resultado = new List<UsuarioAdminViewModel>();
        foreach (var usuario in usuarios)
        {
            var roles = await _userManager.GetRolesAsync(usuario);
            resultado.Add(new UsuarioAdminViewModel
            {
                Id = usuario.Id,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellido}",
                Email = usuario.Email ?? usuario.UserName ?? "Sin correo",
                Rol = roles.Count == 0 ? "Sin rol" : string.Join(", ", roles.Order()),
                Bloqueado = EstaBloqueado(usuario)
            });
        }
        return View("~/Views/Admin/Usuarios.cshtml", new AdminUsuariosViewModel { Usuarios = resultado, Query = q, Pagina = pagina, TotalPaginas = totalPaginas });
    }

    [HttpGet("Crear")]
    public IActionResult Crear() => View("~/Views/Admin/CrearUsuario.cshtml", new CrearUsuarioAdminViewModel { RolesDisponibles = RolesPermitidos, Rol = "Cliente" });

    [HttpPost("Crear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioAdminViewModel model)
    {
        ValidarRol(model.Rol);
        if (await _userManager.FindByEmailAsync(model.Email.Trim()) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo electrónico.");
        }
        if (!ModelState.IsValid) return View("~/Views/Admin/CrearUsuario.cshtml", PrepararModelo(model));

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        var usuario = new ApplicationUser
        {
            UserName = model.Email.Trim(), Email = model.Email.Trim(), Nombre = model.Nombre.Trim(), Apellido = model.Apellido.Trim(),
            Direccion = Limpiar(model.Direccion), LockoutEnabled = true
        };
        var resultadoCrear = await _userManager.CreateAsync(usuario, model.Password);
        if (!resultadoCrear.Succeeded)
        {
            AgregarErrores(resultadoCrear);
            await transaccion.RollbackAsync();
            return View("~/Views/Admin/CrearUsuario.cshtml", PrepararModelo(model));
        }
        var resultadoRol = await _userManager.AddToRoleAsync(usuario, model.Rol);
        if (!resultadoRol.Succeeded)
        {
            AgregarErrores(resultadoRol);
            await transaccion.RollbackAsync();
            return View("~/Views/Admin/CrearUsuario.cshtml", PrepararModelo(model));
        }
        await SincronizarPerfilVendedorAsync(usuario, Array.Empty<string>(), model.Rol);
        await _db.SaveChangesAsync();
        await transaccion.CommitAsync();
        TempData["Mensaje"] = $"Se creó la cuenta de {usuario.Email}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet("Detalle/{id}")]
    public async Task<IActionResult> Detalle(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(usuario);
        var vendedor = await _db.Vendedores.AsNoTracking().FirstOrDefaultAsync(item => item.ApplicationUserId == id);
        return View("~/Views/Admin/DetalleUsuario.cshtml", new UsuarioDetalleAdminViewModel
        {
            Id = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email ?? "Sin correo", Direccion = usuario.Direccion,
            Rol = roles.Count == 0 ? "Sin rol" : string.Join(", ", roles.Order()), Bloqueado = EstaBloqueado(usuario), Tienda = vendedor?.NombreTienda
        });
    }

    [HttpGet("Editar/{id}")]
    public async Task<IActionResult> Editar(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        var roles = await _userManager.GetRolesAsync(usuario);
        return View("~/Views/Admin/EditarUsuario.cshtml", CrearModeloEdicion(usuario, roles.FirstOrDefault() ?? "Cliente"));
    }

    [HttpPost("Editar/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(string id, EditarUsuarioAdminViewModel model)
    {
        if (id != model.Id) return NotFound();
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        ValidarRol(model.Rol);
        var solicitaCambioPassword = !string.IsNullOrWhiteSpace(model.NuevaPassword) || !string.IsNullOrWhiteSpace(model.ConfirmarNuevaPassword);
        if (solicitaCambioPassword)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaPassword) || model.NuevaPassword.Length < 6)
            {
                ModelState.AddModelError(nameof(model.NuevaPassword), "La nueva contraseña debe tener al menos 6 caracteres.");
            }
            if (!string.Equals(model.NuevaPassword, model.ConfirmarNuevaPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.ConfirmarNuevaPassword), "La contraseña y la confirmación no coinciden.");
            }
        }
        if (!string.Equals(usuario.Email, model.Email.Trim(), StringComparison.OrdinalIgnoreCase) && await _userManager.FindByEmailAsync(model.Email.Trim()) != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Ya existe una cuenta con este correo electrónico.");
        }
        var administradorActual = await _userManager.GetUserAsync(User);
        var rolesActuales = await _userManager.GetRolesAsync(usuario);
        if (administradorActual?.Id == usuario.Id && model.Rol != "Administrador") ModelState.AddModelError(string.Empty, "No puedes retirarte tu propio rol de administrador.");
        if (rolesActuales.Contains("Administrador") && model.Rol != "Administrador" && (await _userManager.GetUsersInRoleAsync("Administrador")).Count == 1) ModelState.AddModelError(string.Empty, "Debe existir al menos un administrador en el sistema.");
        if (!ModelState.IsValid) return View("~/Views/Admin/EditarUsuario.cshtml", PrepararModelo(model));

        await using var transaccion = await _db.Database.BeginTransactionAsync();
        usuario.Nombre = model.Nombre.Trim(); usuario.Apellido = model.Apellido.Trim(); usuario.Direccion = Limpiar(model.Direccion);
        usuario.Email = model.Email.Trim(); usuario.UserName = model.Email.Trim();
        var resultadoUsuario = await _userManager.UpdateAsync(usuario);
        if (!resultadoUsuario.Succeeded)
        {
            AgregarErrores(resultadoUsuario); await transaccion.RollbackAsync();
            return View("~/Views/Admin/EditarUsuario.cshtml", PrepararModelo(model));
        }
        if (solicitaCambioPassword)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultadoPassword = await _userManager.ResetPasswordAsync(usuario, token, model.NuevaPassword!);
            if (!resultadoPassword.Succeeded)
            {
                AgregarErrores(resultadoPassword); await transaccion.RollbackAsync();
                return View("~/Views/Admin/EditarUsuario.cshtml", PrepararModelo(model));
            }
        }
        var resultadoRol = await ReemplazarRolAsync(usuario, rolesActuales, model.Rol);
        if (!resultadoRol.Succeeded)
        {
            AgregarErrores(resultadoRol); await transaccion.RollbackAsync();
            return View("~/Views/Admin/EditarUsuario.cshtml", PrepararModelo(model));
        }
        await SincronizarPerfilVendedorAsync(usuario, rolesActuales, model.Rol);
        await _db.SaveChangesAsync();
        await _userManager.UpdateSecurityStampAsync(usuario);
        await transaccion.CommitAsync();
        TempData["Mensaje"] = $"Los datos de {usuario.Email} se actualizaron correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("CambiarBloqueo/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarBloqueo(string id)
    {
        var usuario = await _userManager.FindByIdAsync(id);
        if (usuario == null) return NotFound();
        if ((await _userManager.GetUserAsync(User))?.Id == usuario.Id) { TempData["Error"] = "No puedes bloquear tu propia cuenta."; return RedirectToAction(nameof(Index)); }
        var bloqueado = EstaBloqueado(usuario);
        if (!bloqueado && await _userManager.IsInRoleAsync(usuario, "Administrador") && (await _userManager.GetUsersInRoleAsync("Administrador")).Count == 1) { TempData["Error"] = "No puedes bloquear al único administrador del sistema."; return RedirectToAction(nameof(Index)); }
        usuario.LockoutEnabled = true; usuario.LockoutEnd = bloqueado ? null : DateTimeOffset.UtcNow.AddYears(100);
        var resultado = await _userManager.UpdateAsync(usuario);
        if (resultado.Succeeded) await _userManager.UpdateSecurityStampAsync(usuario);
        TempData[resultado.Succeeded ? "Mensaje" : "Error"] = resultado.Succeeded ? (bloqueado ? "Cuenta desbloqueada correctamente." : "Cuenta bloqueada correctamente.") : "No se pudo actualizar el estado de la cuenta.";
        return RedirectToAction(nameof(Index));
    }

    private void ValidarRol(string rol)
    {
        if (!RolesPermitidos.Contains(rol, StringComparer.Ordinal)) ModelState.AddModelError(nameof(UsuarioFormularioViewModel.Rol), "El rol seleccionado no es válido.");
    }

    private async Task<IdentityResult> ReemplazarRolAsync(ApplicationUser usuario, IList<string> rolesActuales, string nuevoRol)
    {
        var eliminar = rolesActuales.Where(rol => rol != nuevoRol).ToArray();
        if (eliminar.Length > 0)
        {
            var resultado = await _userManager.RemoveFromRolesAsync(usuario, eliminar);
            if (!resultado.Succeeded) return resultado;
        }
        return rolesActuales.Contains(nuevoRol) ? IdentityResult.Success : await _userManager.AddToRoleAsync(usuario, nuevoRol);
    }

    private async Task SincronizarPerfilVendedorAsync(ApplicationUser usuario, IEnumerable<string> rolesAnteriores, string nuevoRol)
    {
        var perfil = await _db.Vendedores.FirstOrDefaultAsync(vendedor => vendedor.ApplicationUserId == usuario.Id);
        if (nuevoRol == "Vendedor")
        {
            if (perfil == null) _db.Vendedores.Add(new Vendedor { ApplicationUserId = usuario.Id, NombreTienda = $"{usuario.Nombre} {usuario.Apellido} - Tienda", Activo = true, FechaRegistro = DateTime.UtcNow });
            else { perfil.Activo = true; perfil.FechaRegistro ??= DateTime.UtcNow; }
        }
        else if (rolesAnteriores.Contains("Vendedor") && perfil != null) perfil.Activo = false;
    }

    private void AgregarErrores(IdentityResult resultado) { foreach (var error in resultado.Errors) ModelState.AddModelError(string.Empty, error.Description); }
    private static bool EstaBloqueado(ApplicationUser usuario) => usuario.LockoutEnd is { } bloqueo && bloqueo > DateTimeOffset.UtcNow;
    private static string? Limpiar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    private static T PrepararModelo<T>(T model) where T : UsuarioFormularioViewModel { model.RolesDisponibles = RolesPermitidos; return model; }
    private static EditarUsuarioAdminViewModel CrearModeloEdicion(ApplicationUser usuario, string rol) => new() { Id = usuario.Id, Nombre = usuario.Nombre, Apellido = usuario.Apellido, Email = usuario.Email ?? "", Direccion = usuario.Direccion, Rol = rol, RolesDisponibles = RolesPermitidos };
}
