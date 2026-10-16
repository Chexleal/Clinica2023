namespace ClinicaDomain;

public partial class Hospital : Base
{
    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    /// <summary>Claves de funciones habilitadas para este hospital.</summary>
    public List<string> Features { get; set; } = new();

    public bool TieneFeature(string feature) => Features.Contains(feature, StringComparer.OrdinalIgnoreCase);

    public ICollection<Clinica> Clinicas { get; set; } = new List<Clinica>();

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
    }
}
