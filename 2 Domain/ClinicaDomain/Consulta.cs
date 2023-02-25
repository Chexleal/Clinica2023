using System;
using System.Collections.Generic;

namespace clinicaWeb.Models;

public partial class Consulta
{
    public decimal IdConsulta { get; set; }

    public decimal? IdPaciente { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Peso { get; set; }

    public TimeSpan? TiempoDuracion { get; set; }

    public bool? Radiografias { get; set; }

    public string? PresionArterial { get; set; }

    public string? Temperatura { get; set; }

    public string? NoRegistro { get; set; }

    public string? MotivoConsulta { get; set; }

    public string? Diagnostico { get; set; }

    public string? Observaciones { get; set; }

    public decimal? IdEmpleado { get; set; }

    public bool? Pagada { get; set; }

    public decimal? Total { get; set; }

    public bool? Terminada { get; set; }

    public string? Glucosa { get; set; }

    public virtual Empleado? IdEmpleadoNavigation { get; set; }

    public virtual Paciente? IdPacienteNavigation { get; set; }

    public virtual ICollection<Receta> Receta { get; } = new List<Receta>();
}
