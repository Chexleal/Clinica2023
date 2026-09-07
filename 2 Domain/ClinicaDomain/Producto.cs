namespace ClinicaDomain;

public partial class Producto : Base
{
    public Guid IdProducto { get; set; }

    public string? Sku { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public Guid IdCategoriaProducto { get; set; }

    public string? UnidadMedida { get; set; }

    public decimal PrecioVenta { get; set; }

    public decimal CostoUltimo { get; set; }

    public decimal StockActual { get; set; }

    public decimal StockMinimo { get; set; }

    /// <summary>Si false, la venta/entrada no pide lote. Si true, exige lote.</summary>
    public bool RequiereLote { get; set; }

    /// <summary>Si false, el lote no pide vencimiento. Solo aplica si RequiereLote.</summary>
    public bool RequiereVencimiento { get; set; }

    /// <summary>LEGADO (columna aún en BD): antes permitía cobrar sin stock. Ya sin uso; todo producto requiere stock.</summary>
    public bool EsSobrePedido { get; set; }

    public bool Activo { get; set; } = true;

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
        Sku ??= string.Empty;
        UnidadMedida ??= string.Empty;
    }
}
