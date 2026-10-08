namespace ClinicaDomain;

public partial class Hospital : Base
{
    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public ICollection<Clinica> Clinicas { get; set; } = new List<Clinica>();

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
    }
}
