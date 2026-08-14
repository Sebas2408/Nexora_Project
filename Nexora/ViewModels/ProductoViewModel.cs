namespace Nexora.ViewModels
{
    public class ProductoViewModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string? Modelo { get; set; }
        public string Especificaciones { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public string CategoriaNombre { get; set; } = string.Empty;
        public string NombreTienda { get; set; } = string.Empty;
    }
}

