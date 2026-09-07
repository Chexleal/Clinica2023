namespace ClinicaDomain;

public partial class Venta : Base
{
    public Guid IdVenta { get; set; }

    public string Folio { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    /// <summary>Opcional: venta de mostrador sin paciente.</summary>
    public Guid? IdPaciente { get; set; }

    /// <summary>Opcional: null = venta libre. Con valor = venta atada a consulta (vía PagarConsulta).</summary>
    public Guid? IdConsulta { get; set; }

    public decimal Total { get; set; }

    /// <summary>Pendiente | Pendiente de pago | Pagada | Anulada</summary>
    public string Estado { get; set; } = "Pendiente";

    public string? Observaciones { get; set; }

    /// <summary>Pendiente de pago: quién queda debiendo el saldo.</summary>
    public string? FiadoResponsable { get; set; }

    /// <summary>Pendiente de pago: fecha promesa de pago (opcional).</summary>
    public DateTime? FechaPromesa { get; set; }
}
