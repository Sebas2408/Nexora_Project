using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Models
{
    public class Vendedor
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string NombreTienda { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descripcion { get; set; }

        [StringLength(250)]
        public string? Logo { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        // Relación 1 a 1 con ApplicationUser
        public ApplicationUser? ApplicationUser { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Producto>? Productos { get; set; }
    }
}
