using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 200 caracteres.")]
        [Display(Name = "Nombre del producto")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La marca es obligatoria.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "La marca debe tener entre 2 y 100 caracteres.")]
        [Display(Name = "Marca")]
        public string Marca { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "El modelo no puede superar los 100 caracteres.")]
        [Display(Name = "Modelo")]
        public string? Modelo { get; set; }

        [Required(ErrorMessage = "Las especificaciones son obligatorias.")]
        [StringLength(2000, MinimumLength = 5, ErrorMessage = "Las especificaciones deben tener entre 5 y 2000 caracteres.")]
        [Display(Name = "Especificaciones")]
        public string Especificaciones { get; set; } = string.Empty;

        [Required(ErrorMessage = "El SKU es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El SKU debe tener entre 3 y 100 caracteres.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9_-]*$", ErrorMessage = "El SKU solo puede contener letras, números, guiones y guiones bajos.")]
        [Display(Name = "SKU")]
        public string SKU { get; set; } = string.Empty; // índice único en DbContext

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "El precio debe estar entre 0,01 y 9.999.999.")]
        [Display(Name = "Precio")]
        public decimal Precio { get; set; }

        [Column(TypeName = "decimal(2,1)")]
        [Range(0, 5)]
        public decimal Rating { get; set; }

        [Required(ErrorMessage = "El stock es obligatorio.")]
        [Range(0, 1000000, ErrorMessage = "El stock debe ser un número entero entre 0 y 1.000.000.")]
        [Display(Name = "Stock")]
        public int Stock { get; set; }

        [Range(0, 120, ErrorMessage = "La garantía debe estar entre 0 y 120 meses.")]
        [Display(Name = "Garantía (meses)")]
        public int GarantiaMeses { get; set; }

        [StringLength(500, ErrorMessage = "La imagen no puede superar los 500 caracteres.")]
        [Display(Name = "Imagen")]
        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        // FK a Categoria
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoría válida.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }

        // FK a Vendedor
        [Range(1, int.MaxValue, ErrorMessage = "Selecciona un vendedor válido.")]
        [Display(Name = "Vendedor")]
        public int VendedorId { get; set; }
        public Vendedor? Vendedor { get; set; }
    }
}
