namespace ClinicaDomain;

public partial class Hospital : Base
{
    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    /// <summary>Estado explícito de cada función configurable del hospital.</summary>
    public Dictionary<string, bool> Features { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool TieneFeature(string feature) => Features.TryGetValue(feature, out var habilitada) && habilitada;

    public ICollection<Clinica> Clinicas { get; set; } = new List<Clinica>();

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
    }
}

public static class HospitalFeatures
{
    public const string VentasSinInventario = "ventas_sin_inventario";
}
