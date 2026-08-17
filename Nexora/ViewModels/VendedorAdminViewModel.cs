namespace Nexora.ViewModels
{
    // Datos resumidos de un vendedor para la tabla del panel admin
    public class VendedorAdminViewModel
    {
        public int Id { get; set; }
        public string NombreTienda { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public int CantidadProductos { get; set; }
    }
}
