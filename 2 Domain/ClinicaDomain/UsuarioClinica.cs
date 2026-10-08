namespace ClinicaDomain;

/// <summary>Acceso de un usuario global a una clínica. El mismo login puede operar en N clínicas.</summary>
public partial class UsuarioClinica : Base
{
    public Guid IdUsuarioClinica { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid IdClinica { get; set; }

    /// <summary>Clínica seleccionada por defecto al iniciar sesión.</summary>
    public bool EsDefault { get; set; }

    public bool Activo { get; set; } = true;

    public virtual Usuario? Usuario { get; set; }

    public virtual Clinica? Clinica { get; set; }
}
