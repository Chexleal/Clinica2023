namespace ClinicaDomain;

public partial class MetodoPago : Base, IHospitalTenant
{
    public Guid IdMetodoPago { get; set; }

    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    /// <summary>Si true, el pago exige número de referencia/autorización.</summary>
    public bool RequiereReferencia { get; set; }

    public bool Activo { get; set; } = true;
}
