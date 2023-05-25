using System;
using System.Collections.Generic;

namespace ClinicaDomain;

public partial class Cita
{
    public Guid IdCita { get; set; }

    public Guid IdPaciente { get; set; }

    public Guid IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public String Titulo {get; set; }


}
