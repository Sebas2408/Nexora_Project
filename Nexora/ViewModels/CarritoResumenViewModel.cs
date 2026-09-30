namespace Nexora.ViewModels;

public sealed class CarritoResumenViewModel
{
    public List<CarritoItemViewModel> Items { get; init; } = new();
    public decimal Subtotal => Items.Sum(item => item.Subtotal);
    public decimal Envio { get; init; }
    public decimal Impuestos { get; init; }
    public decimal Total => Subtotal + Envio + Impuestos;
    public int CantidadUnidades => Items.Sum(item => item.Cantidad);
    public IReadOnlyList<string> Ajustes { get; init; } = Array.Empty<string>();
}
