namespace ClinicaDomain;

/// <summary>
/// Cotización de mostrador: espejo de Venta pero SIN pagos y SIN movimientos de inventario.
/// Estados: Borrador | Vigente | Vencida | Aceptada | Rechazada | Anulada.
/// Solo Borrador/Vigente se editan. Convertir genera una Venta libre Pendiente.
/// </summary>
public partial class Cotizacion : Base, IClinicaTenant, IHospitalTenant
{
    public Guid IdCotizacion { get; set; }

    public Guid IdClinica { get; set; }

    public Guid IdHospital { get; set; }

    /// <summary>Correlativo propio (COT-...), separado de los folios de venta.</summary>
    public string Folio { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    /// <summary>Días de vigencia al emitir. Por defecto 15.</summary>
    public int VigenciaDias { get; set; } = 15;

    /// <summary>Se fija al emitir: Fecha + VigenciaDias.</summary>
    public DateTime? FechaVence { get; set; }

    /// <summary>Opcional: paciente registrado. Null = mostrador sin paciente.</summary>
    public Guid? IdPaciente { get; set; }

    /// <summary>Nombre libre cuando no hay paciente registrado.</summary>
    public string? ClienteNombre { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = "Borrador";

    public string? Observaciones { get; set; }

    /// <summary>Venta generada al convertir (trazabilidad).</summary>
    public Guid? IdVentaConvertida { get; set; }

    public void BeforeSaveChanges()
    {
        Folio ??= string.Empty;
        Estado ??= "Borrador";
        ClienteNombre ??= string.Empty;
        Observaciones ??= string.Empty;
    }
}
