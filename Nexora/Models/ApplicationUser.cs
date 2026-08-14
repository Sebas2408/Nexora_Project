using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Nexora.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Apellido { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Direccion { get; set; }
    }
}
