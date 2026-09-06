using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicaDomain;

/// <summary>
/// Propiedades comunes para todas las entidades persistidas.
/// </summary>
public abstract class Base
{
    public DateTime? FechaCreacion { get; set; }
    public Guid? CreadoPor { get; set; }
    public DateTime? FechaModificacion { get; set; }
    public Guid? ModificadoPor { get; set; }
    public DateTime? FechaEliminacion { get; set; }
    public Guid? EliminadoPor { get; set; }

    // Solo lectura: se llenan en memoria (nunca en BD) resolviendo el Guid contra Usuarios.
    [NotMapped]
    public string? CreadoPorNombre { get; set; }
    [NotMapped]
    public string? ModificadoPorNombre { get; set; }
}
