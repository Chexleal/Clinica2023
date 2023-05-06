using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Consulta
{
    public Guid IdConsulta { get; set; }

    public Guid IdPaciente { get; set; }

    public DateTime Fecha { get; set; }

    public string Peso { get; set; }

    public DateTime TiempoDuracion { get; set; }

    public bool Radiografias { get; set; }

    public string PresionArterial { get; set; }

    public string Temperatura { get; set; }

    public string MotivoConsulta { get; set; }

    public string Diagnostico { get; set; }

    public string Observaciones { get; set; }

    public bool Pagada { get; set; }

    public decimal Total { get; set; }

    public bool Terminada { get; set; }

    //public virtual ICollection<DetalleCobro> DetalleCobros { get; } = new List<DetalleCobro>();

    public Paciente PacienteInformacion { get; set; }

    public virtual ICollection<Receta> Receta { get; } = new List<Receta>();

    public void BeforeSaveChanges()
    {
        Peso ??= string.Empty;
        PresionArterial ??= string.Empty;
        Temperatura ??= string.Empty;
        MotivoConsulta ??= string.Empty;
        Diagnostico ??= string.Empty;
        Observaciones ??= string.Empty;
    }
}
