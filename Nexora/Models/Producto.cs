using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Marca { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Modelo { get; set; }

        [Required]
        public string Especificaciones { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string SKU { get; set; } = string.Empty; // índice único en DbContext

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 9999999)]
        public decimal Precio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Range(0, 120)]
        public int GarantiaMeses { get; set; }

        [StringLength(500)]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        // FK a Categoria
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // FK a Vendedor
        public int VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}
