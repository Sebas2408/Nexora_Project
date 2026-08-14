using System.ComponentModel.DataAnnotations;

namespace Nexora.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Icono { get; set; }
    }
}
