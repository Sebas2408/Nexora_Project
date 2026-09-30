using Nexora.Models;

namespace Nexora.ViewModels;

public sealed class PedidoResumenViewModel
{
    public int Id { get; init; }
    public DateTime FechaCreacion { get; init; }
    public EstadoEnvio Estado { get; init; }
    public decimal Total { get; init; }
    public int CantidadArticulos { get; init; }
}

public sealed class PedidoDetalleViewModel
{
    public int Id { get; init; }
    public DateTime FechaCreacion { get; init; }
    public EstadoEnvio Estado { get; init; }
    public string DireccionEnvio { get; init; } = string.Empty;
    public string MetodoPago { get; init; } = string.Empty;
    public decimal Total { get; init; }
    public List<PedidoLineaViewModel> Lineas { get; init; } = new();
}

public sealed class PedidoLineaViewModel
{
    public string NombreProducto { get; init; } = string.Empty;
    public int Cantidad { get; init; }
    public decimal PrecioUnitario { get; init; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}
