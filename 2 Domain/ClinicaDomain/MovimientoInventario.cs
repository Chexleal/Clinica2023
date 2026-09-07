namespace ClinicaDomain;

public partial class MovimientoInventario : Base
{
    public Guid IdMovimiento { get; set; }

    public DateTime Fecha { get; set; }

    public Guid IdProducto { get; set; }

    /// <summary>Opcional. Solo cuando el producto usa lotes.</summary>
    public Guid? IdLote { get; set; }

    /// <summary>EntradaCompra | SalidaVenta | Ajuste | Merma | Devolucion</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Positivo para entradas, negativo para salidas. Se normaliza en el servicio.</summary>
    public decimal Cantidad { get; set; }

    public decimal CostoUnitario { get; set; }

    public Guid? IdVenta { get; set; }

    public Guid? IdConsulta { get; set; }

    public string? Motivo { get; set; }
}
