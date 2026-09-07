namespace ClinicaDomain;

public partial class VentaPago : Base
{
    public Guid IdVentaPago { get; set; }

    public Guid IdVenta { get; set; }

    public Guid IdMetodoPago { get; set; }

    public decimal Monto { get; set; }

    public string? Referencia { get; set; }

    public DateTime Fecha { get; set; }
}
