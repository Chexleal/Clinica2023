
using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Consulta
{
    public Guid IdConsulta { get; set; }

    public Guid? IdPaciente { get; set; }

    public DateTime Fecha { get; set; }

    public string Peso { get; set; } = null!;

    public DateTime TiempoDuracion { get; set; }

    public bool Radiografias { get; set; }

    public string PresionArterial { get; set; } = null!;

    public string Temperatura { get; set; } = null!;

    public string NoRegistro { get; set; } = null!;

    public string MotivoConsulta { get; set; } = null!;

    public string Diagnostico { get; set; } = null!;

    public string Observaciones { get; set; } = null!;

    public bool Pagada { get; set; }

    public decimal Total { get; set; }

    public bool Terminada { get; set; }

    public virtual ICollection<DetalleCobro> DetalleCobros { get; } = new List<DetalleCobro>();

    public virtual Paciente? IdPacienteNavigation { get; set; }

    public virtual ICollection<Receta> Receta { get; } = new List<Receta>();
}
