using Nexora.Models;

namespace Nexora.ViewModels
{
    // Datos completos de un vendedor para la vista de detalle del admin
    public class VendedorDetalleViewModel
    {
        public int Id { get; set; }
        public string NombreTienda { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Logo { get; set; }
        public bool Activo { get; set; }

        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Direccion { get; set; }

        // Catálogo de productos publicados por el vendedor
        public List<Producto> Productos { get; set; } = new();
    }
}
