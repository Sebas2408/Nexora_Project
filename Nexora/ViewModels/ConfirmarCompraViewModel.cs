using System.ComponentModel.DataAnnotations;

namespace Nexora.ViewModels;

public sealed class ConfirmarCompraViewModel
{
    [Required(ErrorMessage = "La dirección de envío es obligatoria.")]
    [StringLength(500)]
    [Display(Name = "Dirección de envío")]
    public string DireccionEnvio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona un método de pago.")]
    [StringLength(100)]
    [Display(Name = "Método de pago")]
    public string MetodoPago { get; set; } = string.Empty;

    public CarritoResumenViewModel Carrito { get; set; } = new();
}
