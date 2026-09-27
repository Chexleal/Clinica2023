namespace ClinicaDomain;

public partial class CategoriaGasto : Base
{
    public Guid IdCategoriaGasto { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;

    public void BeforeSaveChanges()
    {
        Nombre ??= string.Empty;
    }
}
