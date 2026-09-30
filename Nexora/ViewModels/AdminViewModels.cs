using System.ComponentModel.DataAnnotations;
using Nexora.Models;

namespace Nexora.ViewModels
{
    public class AdminVendedoresViewModel
    {
        public List<VendedorAdminViewModel> Vendedores { get; set; } = new();
        public string? Query { get; set; }
        public bool? Activo { get; set; }
        public string Orden { get; set; } = "tienda";
        public bool Ascendente { get; set; } = true;
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
        public int TotalResultados { get; set; }
        public int TotalVendedores { get; set; }
        public int TotalActivos { get; set; }
        public int TotalSuspendidos { get; set; }
        public int TotalPaginas => (int)Math.Ceiling(TotalResultados / (double)TamanoPagina);
    }

    public class RegistroVendedorViewModel
    {
        [Required, StringLength(150)]
        [Display(Name = "Nombre de la tienda o empresa")]
        public string NombreTienda { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Descripción de la empresa")]
        public string? Descripcion { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Nombre de contacto")]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Apellido de contacto")]
        public string Apellido { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(256)]
        [Display(Name = "Correo electrónico")]
        public string Email { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }

        [Required, StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password))]
        [DataType(DataType.Password)]
        [Display(Name = "Confirmar contraseña")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class EditarVendedorViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Nombre de la tienda o empresa")]
        public string NombreTienda { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Descripción de la empresa")]
        public string? Descripcion { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Nombre de contacto")]
        public string Nombre { get; set; } = string.Empty;

        [Required, StringLength(50)]
        [Display(Name = "Apellido de contacto")]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(250)]
        [Display(Name = "Dirección")]
        public string? Direccion { get; set; }
    }

    public class AdminInventarioViewModel
    {
        public List<AdminProductoViewModel> Productos { get; set; } = new();
        public List<Categoria> Categorias { get; set; } = new();
        public List<AdminVendedorOpcionViewModel> Vendedores { get; set; } = new();
        public string? Query { get; set; }
        public int? VendedorId { get; set; }
        public int? CategoriaId { get; set; }
        public string Estado { get; set; } = "todos";
        public string Existencias { get; set; } = "todas";
        [Range(1, 10000)]
        public int UmbralStockBajo { get; set; } = 5;
        public int Pagina { get; set; } = 1;
        public int TamanoPagina { get; set; } = 10;
        public int TotalResultados { get; set; }
        public int TotalPaginas => (int)Math.Ceiling(TotalResultados / (double)TamanoPagina);
    }

    public class AdminProductoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string SKU { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        public int CategoriaId { get; set; }
        public int Stock { get; set; }
        public bool Activo { get; set; }
        public decimal Precio { get; set; }
    }

    public class AdminProductoDetalleViewModel : AdminProductoViewModel
    {
        public string Marca { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string? Especificaciones { get; set; }
        public string? ImagenUrl { get; set; }
        public int GarantiaMeses { get; set; }
    }

    public class AdminVendedorOpcionViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }

    public class AdminAnaliticasViewModel
    {
        [DataType(DataType.Date)]
        public DateTime? Desde { get; set; }
        [DataType(DataType.Date)]
        public DateTime? Hasta { get; set; }
        public int TotalVendedores { get; set; }
        public int VendedoresActivos { get; set; }
        public int VendedoresSuspendidos { get; set; }
        public int TotalProductos { get; set; }
        public int ProductosActivos { get; set; }
        public int ProductosInactivos { get; set; }
        public int ExistenciasBajas { get; set; }
        public int SinExistencias { get; set; }
        public int UmbralStockBajo { get; set; } = 5;
        public int TotalPedidos { get; set; }
        public decimal Ingresos { get; set; }
        public List<AdminEstadoPedidoViewModel> PedidosPorEstado { get; set; } = new();
        public List<AdminTopVendedorViewModel> MejoresVendedores { get; set; } = new();
        public List<AdminTopProductoViewModel> MejoresProductos { get; set; } = new();
    }

    public class AdminEstadoPedidoViewModel
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    public class AdminTopVendedorViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public int UnidadesVendidas { get; set; }
    }

    public class AdminTopProductoViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public int UnidadesVendidas { get; set; }
        public decimal Ingresos { get; set; }
    }

    public class AdminPerfilViewModel
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
