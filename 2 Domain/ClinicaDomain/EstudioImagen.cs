namespace ClinicaDomain;

public partial class EstudioImagen : Base
{
    public Guid IdEstudio { get; set; }
    public Guid IdPaciente { get; set; }
    public Guid? IdConsulta { get; set; }
    public Guid? IdOrden { get; set; }

    public TipoEstudio Tipo { get; set; }
    public ModalidadDicom Modalidad { get; set; }

    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaEstudio { get; set; }

    public virtual Paciente Paciente { get; set; } = null!;
    public virtual Consulta? Consulta { get; set; }
    public virtual OrdenEstudio? Orden { get; set; }

    public virtual ICollection<ArchivoEstudio> Archivos { get; set; } = new List<ArchivoEstudio>();
}
