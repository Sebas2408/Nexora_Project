using System.ComponentModel.DataAnnotations;

namespace Nexora.ViewModels;

public sealed class AdminUsuariosViewModel
{
    public List<UsuarioAdminViewModel> Usuarios { get; init; } = new();
    public string? Query { get; init; }
    public int Pagina { get; init; }
    public int TotalPaginas { get; init; }
}

public sealed class UsuarioAdminViewModel
{
    public string Id { get; init; } = string.Empty;
    public string NombreCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Rol { get; init; } = "Sin rol";
    public bool Bloqueado { get; init; }
}

public class UsuarioFormularioViewModel
{
    [Required, StringLength(50)]
    public string Nombre { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Apellido { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Required(ErrorMessage = "Selecciona un rol.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = string.Empty;

    public IReadOnlyList<string> RolesDisponibles { get; set; } = Array.Empty<string>();
}

public sealed class CrearUsuarioAdminViewModel : UsuarioFormularioViewModel
{
    [Required, StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare(nameof(Password), ErrorMessage = "La contraseña y la confirmación no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarPassword { get; set; } = string.Empty;
}

public sealed class EditarUsuarioAdminViewModel : UsuarioFormularioViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;
}

public sealed class UsuarioDetalleAdminViewModel
{
    public string Id { get; init; } = string.Empty;
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Direccion { get; init; }
    public string Rol { get; init; } = "Sin rol";
    public bool Bloqueado { get; init; }
    public string? Tienda { get; init; }
}
