namespace ClinicaDomain;

public partial class OrdenEstudio : Base
{
    public Guid IdOrden { get; set; }
    public Guid IdPaciente { get; set; }
    public Guid? IdConsulta { get; set; }

    public TipoEstudio Tipo { get; set; }
    public string Indicacion { get; set; } = string.Empty; // motivo de la orden
    public EstadoOrden Estado { get; set; }
    public DateTime FechaOrden { get; set; }

    public virtual Paciente Paciente { get; set; } = null!;
    public virtual Consulta? Consulta { get; set; }

    public virtual ICollection<EstudioImagen> Estudios { get; set; } = new List<EstudioImagen>();
}
