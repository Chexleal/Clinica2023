using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Cita
{
    public Guid IdCita { get; set; }

    public Guid IdPaciente { get; set; }

    public Guid IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public virtual Paciente IdPacienteNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; }
}
