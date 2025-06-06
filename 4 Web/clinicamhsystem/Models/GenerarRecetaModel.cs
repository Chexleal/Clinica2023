using ClinicaDomain;

namespace clinicaWeb.Models;

public class GenerarRecetaModel
{
    public List<Paciente> Pacientes { get; set; }
    public List<Consulta> Consultas { get; set; }
    public Receta Receta { get; set; }
    public Consulta Consulta { get; set; }
    public Paciente Paciente { get; set; }
    public List<DetalleReceta> DetallesReceta { get; set; }
    public DateTime CitaProx { get; set; }
}
