using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Nexora.Models
{
    public enum EstadoEnvio
    {
        Pendiente,
        Procesando,
        Enviado,
        Entregado,
        Cancelado
    }

    public class Orden
    {
        public int Id { get; set; }

        [Required]
        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? ApplicationUser { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [Required]
        [StringLength(500)]
        public string DireccionEnvio { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string MetodoPago { get; set; } = string.Empty;

        public EstadoEnvio Estado { get; set; } = EstadoEnvio.Pendiente;

        public ICollection<DetalleOrden>? Detalles { get; set; }
    }
}
