namespace Nexora.ViewModels
{
    // Datos resumidos de un vendedor para la tabla del panel admin
    public class VendedorAdminViewModel
    {
        public int Id { get; set; }
        public string NombreTienda { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Dominio { get; set; } = string.Empty;
        public DateTime? FechaRegistro { get; set; }
        public bool Activo { get; set; }
        public int CantidadProductos { get; set; }
        public decimal IngresosHistoricos { get; set; }
    }
}
