namespace ClinicaDomain;

public partial class OrdenEstudio : Base, IClinicaTenant
{
    public Guid IdOrden { get; set; }
    public Guid IdClinica { get; set; }
    public Guid IdPaciente { get; set; }
    public Guid? IdConsulta { get; set; }

    public TipoEstudio Tipo { get; set; }
    public string Indicacion { get; set; } = string.Empty; // motivo de la orden
    public EstadoOrden Estado { get; set; }
    public DateTime FechaOrden { get; set; }
    /// <summary>
    /// True = examen externo (se imprime para el paciente, no se atiende en clínica).
    /// False = examen interno (aparece en la cola de pendientes de Estudios).
    /// </summary>
    public bool EsExterna { get; set; }

    public virtual Paciente Paciente { get; set; } = null!;
    public virtual Consulta? Consulta { get; set; }

    public virtual ICollection<EstudioImagen> Estudios { get; set; } = new List<EstudioImagen>();
}
