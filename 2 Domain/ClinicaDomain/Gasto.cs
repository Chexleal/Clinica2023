namespace ClinicaDomain;

public partial class Gasto : Base
{
    public Guid IdGasto { get; set; }

    /// <summary>Fecha del gasto (criterio de caja y reportes). Distinta de FechaCreacion (auditoría).</summary>
    public DateTime Fecha { get; set; }

    public Guid IdCategoriaGasto { get; set; }

    /// <summary>Concepto del gasto (ej. "Pago de luz junio", "Salario recepcionista").</summary>
    public string Concepto { get; set; } = string.Empty;

    /// <summary>Proveedor o beneficiario (a quién se pagó). Opcional.</summary>
    public string? Proveedor { get; set; }

    /// <summary>Número de factura/recibo de respaldo fiscal. Opcional.</summary>
    public string? NumeroComprobante { get; set; }

    /// <summary>Ruta relativa del comprobante (foto/PDF) en storage. Solo se guarda la ruta, igual que estudios.</summary>
    public string? RutaComprobante { get; set; }

    /// <summary>Nombre original del archivo de comprobante, para mostrar/descargar.</summary>
    public string? NombreComprobante { get; set; }

    /// <summary>Cómo se pagó (reutiliza catálogo Metodo_pago). Requerido.</summary>
    public Guid IdMetodoPago { get; set; }

    /// <summary>Referencia del pago (no. cheque/transferencia/autorización). Opcional.</summary>
    public string? Referencia { get; set; }

    public decimal Monto { get; set; }

    public string? Observaciones { get; set; }

    /// <summary>Registrado | Anulado (no se borra: el anulado queda para auditoría, igual que Venta).</summary>
    public string Estado { get; set; } = "Registrado";

    public void BeforeSaveChanges()
    {
        Concepto ??= string.Empty;
        Proveedor ??= string.Empty;
        NumeroComprobante ??= string.Empty;
        RutaComprobante ??= string.Empty;
        NombreComprobante ??= string.Empty;
        Referencia ??= string.Empty;
        Observaciones ??= string.Empty;
        Estado ??= "Registrado";
    }
}
