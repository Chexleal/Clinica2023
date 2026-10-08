namespace ClinicaDomain;

/// <summary>Línea de cotización. Precios copiados al agregar (foto del catálogo, no referencia viva).</summary>
public partial class CotizacionDetalle : Base
{
    public Guid IdCotizacionDetalle { get; set; }

    public Guid IdCotizacion { get; set; }

    /// <summary>Servicio | Producto</summary>
    public string TipoLinea { get; set; } = string.Empty;

    public Guid? IdMotivoCobro { get; set; }

    public Guid? IdProducto { get; set; }

    /// <summary>Referencial/informativo: la cotización no reserva ni descuenta lotes.</summary>
    public Guid? IdLote { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoMonto { get; set; }

    public string? DescuentoMotivo { get; set; }

    public Guid? DescuentoOtorgadoPor { get; set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public string? DescuentoOtorgadoPorNombre { get; set; }

    public bool EsSobrePedido { get; set; }

    public void BeforeSaveChanges()
    {
        TipoLinea ??= string.Empty;
        Descripcion ??= string.Empty;
        DescuentoMotivo ??= string.Empty;
    }
}
