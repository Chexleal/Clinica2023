namespace ClinicaDomain;
public class Aseguradora : Base, IHospitalTenant
{
    public Guid IdAseguradora { get; set; }
    public Guid IdHospital { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? IdentificadorFiscal { get; set; }
    public string? Contacto { get; set; }
    public decimal CopagoDefault { get; set; }
    public decimal CoaseguroPorcDefault { get; set; }
    public bool Activa { get; set; } = true;
    public void BeforeSaveChanges()
    {
        Nombre = (Nombre ?? string.Empty).Trim();
        IdentificadorFiscal = string.IsNullOrWhiteSpace(IdentificadorFiscal) ? null : IdentificadorFiscal.Trim();
        Contacto = string.IsNullOrWhiteSpace(Contacto) ? null : Contacto.Trim();
        CopagoDefault = Math.Max(0, CopagoDefault);
        CoaseguroPorcDefault = Math.Clamp(CoaseguroPorcDefault, 0, 100);
    }
}
