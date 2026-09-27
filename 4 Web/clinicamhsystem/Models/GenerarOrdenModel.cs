using ClinicaDomain;

namespace clinicaWeb.Models;

public class GenerarOrdenModel
{
    public OrdenEstudio Orden { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public Consulta? Consulta { get; set; }
    public string Medico { get; set; } = string.Empty;
}

public class GenerarOrdenesModel
{
    public Paciente Paciente { get; set; } = null!;
    public List<OrdenEstudio> Ordenes { get; set; } = new();
    public string Medico { get; set; } = string.Empty;
}
