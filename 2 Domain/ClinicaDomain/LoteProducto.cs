namespace ClinicaDomain;

public partial class LoteProducto : Base
{
    public Guid IdLote { get; set; }

    public Guid IdProducto { get; set; }

    public string? CodigoLote { get; set; }

    /// <summary>Opcional. Solo se exige si Producto.RequiereVencimiento.</summary>
    public DateTime? FechaVencimiento { get; set; }

    public decimal Stock { get; set; }

    public decimal CostoUnitario { get; set; }

    public bool Activo { get; set; } = true;

    public void BeforeSaveChanges()
    {
        CodigoLote ??= string.Empty;
    }
}
