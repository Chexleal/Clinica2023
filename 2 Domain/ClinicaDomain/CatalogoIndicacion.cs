namespace ClinicaDomain;

public partial class CatalogoIndicacion : Base
{
    public Guid IdCatalogo { get; set; }
    public TipoEstudio Tipo { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
