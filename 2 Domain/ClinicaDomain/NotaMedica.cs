namespace ClinicaDomain;

public partial class NotaMedica : Base
{
    public Guid IdNotaMedica { get; set; }
    public Guid IdPaciente { get; set; }
    public Guid IdConsulta { get; set; }

    public string Motivo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
    public DateTime FechaNota { get; set; }

    /// <summary>Imprimir el Motivo de consulta de la consulta en la nota.</summary>
    public bool IncluirMotivo { get; set; } = true;
    /// <summary>Imprimir el Diagnóstico de la consulta en la nota.</summary>
    public bool IncluirDiagnostico { get; set; } = true;

    public virtual Paciente? Paciente { get; set; }
    public virtual Consulta? Consulta { get; set; }
}
