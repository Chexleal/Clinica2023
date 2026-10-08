namespace ClinicaDomain;

public partial class Clinica : Base
{
    public Guid IdClinica { get; set; }

    public Guid IdHospital { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activa { get; set; } = true;

    public virtual Hospital? Hospital { get; set; }

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
    }
}
