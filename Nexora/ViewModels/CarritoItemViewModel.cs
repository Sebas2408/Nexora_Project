using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.ViewModels
{
    public class CarritoItemViewModel
    {
        public int ProductoId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? ImagenUrl { get; set; }
        public decimal Precio { get; set; }

        public int Cantidad { get; set; }

        [NotMapped]
        public decimal Subtotal => Precio * Cantidad;
    }
}
