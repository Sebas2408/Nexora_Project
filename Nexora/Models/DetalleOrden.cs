using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Models
{
    public class DetalleOrden
    {
        public int Id { get; set; }

        [Required]
        public int OrdenId { get; set; }
        public Orden? Orden { get; set; }

        [Required]
        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999)]
        public decimal PrecioUnitario { get; set; }
    }
}
